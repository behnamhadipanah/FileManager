using FileManager.Domain.Aggregates.UserAgg;
using FileManager.Domain.Repositories;
using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;
using FileManager.Infrastructure.Persistence.SqlServer.Tables;
using Microsoft.Data.SqlClient;

namespace FileManager.Infrastructure.Persistence.SqlServer.Repositories;

public sealed class UserRepository(ISqlConnectionFactory connectionFactory) : IUserRepository
{
    private static readonly SqlColumn[] SelectAll =
    [
        Users.Id, Users.FirstName, Users.LastName, Users.Email,
        Users.PasswordHash, Users.IsActive, Users.CreationTime
    ];

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Users.Table} WHERE {Users.Id.Name} = {Users.Id.Parameter}
            """;
        cmd.Add(Users.Id, id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Users.Table}
            WHERE {Users.Email.Name} = {Users.Email.Parameter}
            """;
        cmd.Add(Users.Email, email.Trim().ToLowerInvariant());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM {Users.Table} WHERE {Users.Email.Name} = {Users.Email.Parameter}
            """;
        cmd.Add(Users.Email, email.Trim().ToLowerInvariant());

        return await cmd.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task InsertAsync(User user, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            INSERT INTO {Users.Table}
                ({Users.FirstName.Name}, {Users.LastName.Name}, {Users.Email.Name},
                 {Users.PasswordHash.Name}, {Users.IsActive.Name}, {Users.CreationTime.Name})
            OUTPUT INSERTED.{Users.Id.Name}
            VALUES
                ({Users.FirstName.Parameter}, {Users.LastName.Parameter}, {Users.Email.Parameter},
                 {Users.PasswordHash.Parameter}, {Users.IsActive.Parameter}, {Users.CreationTime.Parameter})
            """;

        cmd.Add(Users.FirstName, user.FirstName);
        cmd.Add(Users.LastName, user.LastName);
        cmd.Add(Users.Email, user.Email);
        cmd.Add(Users.PasswordHash, user.PasswordHash);
        cmd.Add(Users.IsActive, user.IsActive);
        cmd.Add(Users.CreationTime, user.CreationTime);

        var generatedId = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        user.AssignId(generatedId);
    }

    private static User Map(SqlDataReader reader) =>
        User.FromPersistence(
            id: reader.GetInt64(0),
            firstName: reader.GetString(1),
            lastName: reader.GetString(2),
            email: reader.GetString(3),
            passwordHash: reader.GetString(4),
            isActive: reader.GetBoolean(5),
            creationTime: reader.GetDateTime(6));
}
