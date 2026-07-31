using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;
using FileManager.Infrastructure.Persistence.SqlServer.Tables;
using Kootam.Authentication.Abstractions.Models;
using Kootam.Authentication.Abstractions.Services;
using Microsoft.Data.SqlClient;

namespace FileManager.Infrastructure.Authentication;

public sealed class SqlRefreshTokenService(ISqlConnectionFactory connectionFactory)
    : IRefreshTokenService<long>
{
    public async Task StoreAsync(RefreshToken<long> refreshToken, CancellationToken cancellationToken)
    {
        EnsureId(refreshToken);

        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            INSERT INTO {RefreshTokens.Table}
                ({RefreshTokens.Id.Name}, {RefreshTokens.UserId.Name}, {RefreshTokens.Token.Name},
                 {RefreshTokens.Expires.Name}, {RefreshTokens.Created.Name}, {RefreshTokens.CreatedByIp.Name})
            VALUES
                ({RefreshTokens.Id.Parameter}, {RefreshTokens.UserId.Parameter}, {RefreshTokens.Token.Parameter},
                 {RefreshTokens.Expires.Parameter}, {RefreshTokens.Created.Parameter}, {RefreshTokens.CreatedByIp.Parameter})
            """;

        cmd.Add(RefreshTokens.Id, refreshToken.Id);
        cmd.Add(RefreshTokens.UserId, refreshToken.UserId);
        cmd.Add(RefreshTokens.Token, refreshToken.Token);
        cmd.Add(RefreshTokens.Expires, refreshToken.Expires);
        cmd.Add(RefreshTokens.Created, refreshToken.Created);
        cmd.Add(RefreshTokens.CreatedByIp, refreshToken.CreatedByIp);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            UPDATE {RefreshTokens.Table}
            SET {RefreshTokens.Revoked.Name} = {RefreshTokens.Revoked.Parameter}
            WHERE {RefreshTokens.Token.Name} = {RefreshTokens.Token.Parameter}
              AND {RefreshTokens.Revoked.Name} IS NULL
            """;
        cmd.Add(RefreshTokens.Revoked, DateTime.UtcNow);
        cmd.Add(RefreshTokens.Token, token);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<RefreshToken<long>?> FindAsync(string token, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {RefreshTokens.Id.Name}, {RefreshTokens.UserId.Name}, {RefreshTokens.Token.Name},
                   {RefreshTokens.Expires.Name}, {RefreshTokens.Created.Name}, {RefreshTokens.CreatedByIp.Name},
                   {RefreshTokens.Revoked.Name}, {RefreshTokens.RevokedByIp.Name}, {RefreshTokens.ReplacedByToken.Name}
            FROM {RefreshTokens.Table}
            WHERE {RefreshTokens.Token.Name} = {RefreshTokens.Token.Parameter}
            """;
        cmd.Add(RefreshTokens.Token, token);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new RefreshToken<long>
        {
            Id = reader.GetGuid(0),
            UserId = reader.GetInt64(1),
            Token = reader.GetString(2),
            Expires = reader.GetDateTime(3),
            Created = reader.GetDateTime(4),
            CreatedByIp = reader.IsDBNull(5) ? null : reader.GetString(5),
            Revoked = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            RevokedByIp = reader.IsDBNull(7) ? null : reader.GetString(7),
            ReplacedByToken = reader.IsDBNull(8) ? null : reader.GetString(8)
        };
    }

    public async Task RotateAsync(
        RefreshToken<long> current,
        RefreshToken<long> replacement,
        CancellationToken cancellationToken)
    {
        EnsureId(replacement);

        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            await using (var revoke = conn.CreateCommand())
            {
                revoke.Transaction = (SqlTransaction)tx;
                revoke.CommandText = $"""
                    UPDATE {RefreshTokens.Table}
                    SET {RefreshTokens.Revoked.Name} = {RefreshTokens.Revoked.Parameter},
                        {RefreshTokens.ReplacedByToken.Name} = {RefreshTokens.ReplacedByToken.Parameter}
                    WHERE {RefreshTokens.Id.Name} = {RefreshTokens.Id.Parameter}
                    """;
                revoke.Add(RefreshTokens.Revoked, DateTime.UtcNow);
                revoke.Add(RefreshTokens.ReplacedByToken, replacement.Token);
                revoke.Add(RefreshTokens.Id, current.Id);
                await revoke.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var insert = conn.CreateCommand())
            {
                insert.Transaction = (SqlTransaction)tx;
                insert.CommandText = $"""
                    INSERT INTO {RefreshTokens.Table}
                        ({RefreshTokens.Id.Name}, {RefreshTokens.UserId.Name}, {RefreshTokens.Token.Name},
                         {RefreshTokens.Expires.Name}, {RefreshTokens.Created.Name}, {RefreshTokens.CreatedByIp.Name})
                    VALUES
                        ({RefreshTokens.Id.Parameter}, {RefreshTokens.UserId.Parameter}, {RefreshTokens.Token.Parameter},
                         {RefreshTokens.Expires.Parameter}, {RefreshTokens.Created.Parameter}, {RefreshTokens.CreatedByIp.Parameter})
                    """;
                insert.Add(RefreshTokens.Id, replacement.Id);
                insert.Add(RefreshTokens.UserId, replacement.UserId);
                insert.Add(RefreshTokens.Token, replacement.Token);
                insert.Add(RefreshTokens.Expires, replacement.Expires);
                insert.Add(RefreshTokens.Created, replacement.Created);
                insert.Add(RefreshTokens.CreatedByIp, replacement.CreatedByIp);
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static void EnsureId(RefreshToken<long> refreshToken)
    {
        if (refreshToken.Id == Guid.Empty)
            refreshToken.Id = Guid.NewGuid();
    }
}
