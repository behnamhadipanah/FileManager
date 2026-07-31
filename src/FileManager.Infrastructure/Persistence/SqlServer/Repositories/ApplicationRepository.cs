using FileManager.Domain.Aggregates.ApplicationAgg;
using FileManager.Domain.Repositories;
using FileManager.Domain.ValueObjects;
using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;
using FileManager.Infrastructure.Persistence.SqlServer.Tables;
using Kootam.Framework.Domain.ValueObjects;
using Microsoft.Data.SqlClient;

namespace FileManager.Infrastructure.Persistence.SqlServer.Repositories;

public sealed class ApplicationRepository(ISqlConnectionFactory connectionFactory) : IApplicationRepository
{
    private static readonly SqlColumn[] SelectAll =
    [
        Applications.Id, Applications.BusinessId, Applications.ApplicationName, Applications.Token,
        Applications.MinImageSize, Applications.MaxImageSize,
        Applications.MinVideoSize, Applications.MaxVideoSize,
        Applications.MinDocumentSize, Applications.MaxDocumentSize,
        Applications.IsActive, Applications.CreationTime, Applications.CreatorId,
        Applications.LastModificationTime, Applications.LastModifierId
    ];

    public async Task<RegisteredApplication?> GetAsync(long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Applications.Table} WHERE {Applications.Id.Name} = {Applications.Id.Parameter}
            """;
        cmd.Add(Applications.Id, id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<RegisteredApplication?> GetByBusinessIdAsync(BusinessId businessId, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Applications.Table} WHERE {Applications.BusinessId.Name} = {Applications.BusinessId.Parameter}
            """;
        cmd.Add(Applications.BusinessId, (Guid)businessId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<RegisteredApplication?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Applications.Table} WHERE {Applications.Token.Name} = {Applications.Token.Parameter}
            """;
        cmd.Add(Applications.Token, token);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<RegisteredApplication>> GetAllActiveAsync(CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Applications.Table}
            WHERE {Applications.IsActive.Name} = 1
            ORDER BY {Applications.ApplicationName.Name}
            """;

        var results = new List<RegisteredApplication>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(Map(reader));

        return results;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM {Applications.Table} WHERE {Applications.Id.Name} = {Applications.Id.Parameter}
            """;
        cmd.Add(Applications.Id, id);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    public async Task<bool> ExistsByNameAsync(string applicationName, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM {Applications.Table} WHERE {Applications.ApplicationName.Name} = {Applications.ApplicationName.Parameter}
            """;
        cmd.Add(Applications.ApplicationName, applicationName);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    public async Task InsertAsync(RegisteredApplication application, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            INSERT INTO {Applications.Table}
                ({Applications.BusinessId.Name}, {Applications.ApplicationName.Name}, {Applications.Token.Name},
                 {Applications.MinImageSize.Name}, {Applications.MaxImageSize.Name},
                 {Applications.MinVideoSize.Name}, {Applications.MaxVideoSize.Name},
                 {Applications.MinDocumentSize.Name}, {Applications.MaxDocumentSize.Name},
                 {Applications.IsActive.Name}, {Applications.CreationTime.Name}, {Applications.CreatorId.Name})
            OUTPUT INSERTED.{Applications.Id.Name}
            VALUES
                ({Applications.BusinessId.Parameter}, {Applications.ApplicationName.Parameter}, {Applications.Token.Parameter},
                 {Applications.MinImageSize.Parameter}, {Applications.MaxImageSize.Parameter},
                 {Applications.MinVideoSize.Parameter}, {Applications.MaxVideoSize.Parameter},
                 {Applications.MinDocumentSize.Parameter}, {Applications.MaxDocumentSize.Parameter},
                 {Applications.IsActive.Parameter}, {Applications.CreationTime.Parameter}, {Applications.CreatorId.Parameter})
            """;

        cmd.Add(Applications.BusinessId, (Guid)application.BusinessId);
        cmd.Add(Applications.ApplicationName, application.ApplicationName);
        cmd.Add(Applications.Token, application.Token.Value);
        cmd.Add(Applications.MinImageSize, application.UploadLimits.MinImageSizeKilobytes);
        cmd.Add(Applications.MaxImageSize, application.UploadLimits.MaxImageSizeKilobytes);
        cmd.Add(Applications.MinVideoSize, application.UploadLimits.MinVideoSizeKilobytes);
        cmd.Add(Applications.MaxVideoSize, application.UploadLimits.MaxVideoSizeKilobytes);
        cmd.Add(Applications.MinDocumentSize, application.UploadLimits.MinDocumentSizeKilobytes);
        cmd.Add(Applications.MaxDocumentSize, application.UploadLimits.MaxDocumentSizeKilobytes);
        cmd.Add(Applications.IsActive, application.IsActive);
        cmd.Add(Applications.CreationTime, application.CreationTime);
        cmd.Add(Applications.CreatorId, application.CreatorId);

        var generatedId = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        application.AssignId(generatedId);
    }

    public async Task UpdateAsync(RegisteredApplication application, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            UPDATE {Applications.Table}
            SET {SqlSchema.SetClause(
                Applications.ApplicationName, Applications.Token,
                Applications.MinImageSize, Applications.MaxImageSize,
                Applications.MinVideoSize, Applications.MaxVideoSize,
                Applications.MinDocumentSize, Applications.MaxDocumentSize,
                Applications.IsActive, Applications.LastModificationTime, Applications.LastModifierId)}
            WHERE {Applications.Id.Name} = {Applications.Id.Parameter}
            """;

        cmd.Add(Applications.ApplicationName, application.ApplicationName);
        cmd.Add(Applications.Token, application.Token.Value);
        cmd.Add(Applications.MinImageSize, application.UploadLimits.MinImageSizeKilobytes);
        cmd.Add(Applications.MaxImageSize, application.UploadLimits.MaxImageSizeKilobytes);
        cmd.Add(Applications.MinVideoSize, application.UploadLimits.MinVideoSizeKilobytes);
        cmd.Add(Applications.MaxVideoSize, application.UploadLimits.MaxVideoSizeKilobytes);
        cmd.Add(Applications.MinDocumentSize, application.UploadLimits.MinDocumentSizeKilobytes);
        cmd.Add(Applications.MaxDocumentSize, application.UploadLimits.MaxDocumentSizeKilobytes);
        cmd.Add(Applications.IsActive, application.IsActive);
        cmd.Add(Applications.LastModificationTime, application.LastModificationTime);
        cmd.Add(Applications.LastModifierId, application.LastModifierId);
        cmd.Add(Applications.Id, application.Id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static RegisteredApplication Map(SqlDataReader reader)
    {
        var uploadLimits = UploadLimits.FromPersistence(
            reader.GetInt64(4), reader.GetInt64(5),
            reader.GetInt64(6), reader.GetInt64(7),
            reader.GetInt64(8), reader.GetInt64(9));

        return RegisteredApplication.FromPersistence(
            id: reader.GetInt64(0),
            businessId: reader.GetGuid(1),
            applicationName: reader.GetString(2),
            token: ApplicationToken.FromString(reader.GetString(3)),
            uploadLimits: uploadLimits,
            isActive: reader.GetBoolean(10),
            creationTime: reader.GetDateTime(11),
            creatorId: reader.GetInt64(12),
            lastModificationTime: reader.NullableDateTime(13),
            lastModifierId: reader.NullableLong(14));
    }
}
