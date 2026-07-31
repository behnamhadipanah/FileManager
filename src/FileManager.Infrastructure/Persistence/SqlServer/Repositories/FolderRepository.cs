using FileManager.Domain.Aggregates.FolderAgg;
using FileManager.Domain.Repositories;
using FileManager.Domain.ValueObjects;
using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;
using FileManager.Infrastructure.Persistence.SqlServer.Tables;
using Kootam.Framework.Domain.ValueObjects;
using Microsoft.Data.SqlClient;

namespace FileManager.Infrastructure.Persistence.SqlServer.Repositories;

public sealed class FolderRepository(ISqlConnectionFactory connectionFactory) : IFolderRepository
{
    private static readonly SqlColumn[] SelectAll =
    [
        Folders.Id, Folders.BusinessId, Folders.ApplicationId, Folders.ParentFolderId, Folders.Name,
        Folders.IsDeleted, Folders.DeletionTime, Folders.CreationTime, Folders.CreatorId,
        Folders.LastModificationTime, Folders.LastModifierId
    ];

    public async Task<Folder?> GetAsync(long applicationId, long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Folders.Table}
            WHERE {Folders.Id.Name} = {Folders.Id.Parameter} AND {Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
            """;
        cmd.Add(Folders.Id, id);
        cmd.Add(Folders.ApplicationId, applicationId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<Folder?> GetByBusinessIdAsync(long applicationId, BusinessId businessId, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Folders.Table}
            WHERE {Folders.BusinessId.Name} = {Folders.BusinessId.Parameter} AND {Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
            """;
        cmd.Add(Folders.BusinessId, (Guid)businessId);
        cmd.Add(Folders.ApplicationId, applicationId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<Folder?> GetRootAsync(long applicationId, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Folders.Table}
            WHERE {Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
              AND {Folders.ParentFolderId.Name} IS NULL
              AND {Folders.Name.Name} = {Folders.Name.Parameter}
              AND {Folders.IsDeleted.Name} = 0
            """;
        cmd.Add(Folders.ApplicationId, applicationId);
        cmd.Add(Folders.Name, Folder.RootFolderName);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> ExistsAsync(long applicationId, long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM {Folders.Table}
            WHERE {Folders.Id.Name} = {Folders.Id.Parameter} AND {Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
            """;
        cmd.Add(Folders.Id, id);
        cmd.Add(Folders.ApplicationId, applicationId);

        return await cmd.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<bool> ExistsByNameAsync(long applicationId, long? parentFolderId, string name, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM {Folders.Table}
            WHERE {Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
              AND {Folders.Name.Name} = {Folders.Name.Parameter}
              AND {Folders.IsDeleted.Name} = 0
              AND (({Folders.ParentFolderId.Name} IS NULL AND {Folders.ParentFolderId.Parameter} IS NULL) OR {Folders.ParentFolderId.Name} = {Folders.ParentFolderId.Parameter})
            """;
        cmd.Add(Folders.ApplicationId, applicationId);
        cmd.Add(Folders.Name, name);
        cmd.Add(Folders.ParentFolderId, parentFolderId);

        return await cmd.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<bool> HasChildrenAsync(long applicationId, long folderId, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM {Folders.From}
            WHERE {Folders.Alias}.{Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
              AND {Folders.Alias}.{Folders.ParentFolderId.Name} = {Folders.ParentFolderId.Parameter}
              AND {Folders.Alias}.{Folders.IsDeleted.Name} = 0
            UNION ALL
            SELECT 1 FROM {StorageFiles.From}
            WHERE {StorageFiles.Alias}.{StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
              AND {StorageFiles.Alias}.{StorageFiles.ParentFolderId.Name} = {StorageFiles.ParentFolderId.Parameter}
              AND {StorageFiles.Alias}.{StorageFiles.IsDeleted.Name} = 0
            """;
        // Folders.ApplicationId/ParentFolderId and StorageFiles.ApplicationId/ParentFolderId share
        // the same column names, so the same @ApplicationId/@ParentFolderId parameters satisfy both
        // halves of the UNION query - adding them twice would throw a duplicate parameter error.
        cmd.Add(Folders.ApplicationId, applicationId);
        cmd.Add(Folders.ParentFolderId, folderId);

        return await cmd.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<IReadOnlyList<Folder>> GetByParentFolderIdAsync(
        long applicationId,
        long parentFolderId,
        bool? isDeleted,
        CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        var deletedFilter = isDeleted is null
            ? $"{Folders.IsDeleted.Name} = 0"
            : $"{Folders.IsDeleted.Name} = {Folders.IsDeleted.Parameter}";

        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {Folders.Table}
            WHERE {Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
              AND {Folders.ParentFolderId.Name} = {Folders.ParentFolderId.Parameter}
              AND {deletedFilter}
            ORDER BY {Folders.Name.Name}
            """;
        cmd.Add(Folders.ApplicationId, applicationId);
        cmd.Add(Folders.ParentFolderId, parentFolderId);
        if (isDeleted is not null)
            cmd.Add(Folders.IsDeleted, isDeleted.Value);

        var folders = new List<Folder>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            folders.Add(Map(reader));

        return folders;
    }

    public async Task InsertAsync(Folder folder, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            INSERT INTO {Folders.Table}
                ({Folders.BusinessId.Name}, {Folders.ApplicationId.Name}, {Folders.ParentFolderId.Name}, {Folders.Name.Name},
                 {Folders.IsDeleted.Name}, {Folders.CreationTime.Name}, {Folders.CreatorId.Name})
            OUTPUT INSERTED.{Folders.Id.Name}
            VALUES
                ({Folders.BusinessId.Parameter}, {Folders.ApplicationId.Parameter}, {Folders.ParentFolderId.Parameter}, {Folders.Name.Parameter},
                 {Folders.IsDeleted.Parameter}, {Folders.CreationTime.Parameter}, {Folders.CreatorId.Parameter})
            """;

        cmd.Add(Folders.BusinessId, (Guid)folder.BusinessId);
        cmd.Add(Folders.ApplicationId, folder.ApplicationId);
        cmd.Add(Folders.ParentFolderId, folder.ParentFolderId);
        cmd.Add(Folders.Name, folder.Name.Value);
        cmd.Add(Folders.IsDeleted, folder.IsDeleted);
        cmd.Add(Folders.CreationTime, folder.CreationTime);
        cmd.Add(Folders.CreatorId, folder.CreatorId);

        var generatedId = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        folder.AssignId(generatedId);
    }

    public async Task UpdateAsync(Folder folder, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            UPDATE {Folders.Table}
            SET {SqlSchema.SetClause(
                Folders.ParentFolderId, Folders.Name, Folders.IsDeleted, Folders.DeletionTime,
                Folders.LastModificationTime, Folders.LastModifierId)}
            WHERE {Folders.Id.Name} = {Folders.Id.Parameter} AND {Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
            """;

        cmd.Add(Folders.ParentFolderId, folder.ParentFolderId);
        cmd.Add(Folders.Name, folder.Name.Value);
        cmd.Add(Folders.IsDeleted, folder.IsDeleted);
        cmd.Add(Folders.DeletionTime, folder.DeletionTime);
        cmd.Add(Folders.LastModificationTime, folder.LastModificationTime);
        cmd.Add(Folders.LastModifierId, folder.LastModifierId);
        cmd.Add(Folders.Id, folder.Id);
        cmd.Add(Folders.ApplicationId, folder.ApplicationId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(long applicationId, long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            DELETE FROM {Folders.Table}
            WHERE {Folders.Id.Name} = {Folders.Id.Parameter} AND {Folders.ApplicationId.Name} = {Folders.ApplicationId.Parameter}
            """;
        cmd.Add(Folders.Id, id);
        cmd.Add(Folders.ApplicationId, applicationId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Folder Map(SqlDataReader reader) =>
        Folder.FromPersistence(
            id: reader.GetInt64(0),
            businessId: reader.GetGuid(1),
            applicationId: reader.GetInt64(2),
            parentFolderId: reader.NullableLong(3),
            name: FileName.FromString(reader.GetString(4)),
            isDeleted: reader.GetBoolean(5),
            deletionTime: reader.NullableDateTime(6),
            creationTime: reader.GetDateTime(7),
            creatorId: reader.GetInt64(8),
            lastModificationTime: reader.NullableDateTime(9),
            lastModifierId: reader.NullableLong(10));
}
