using System.Text.Json;
using FileManager.Domain.Aggregates.FileAgg;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Repositories;
using FileManager.Domain.ValueObjects;
using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;
using FileManager.Infrastructure.Persistence.SqlServer.Tables;
using Kootam.Framework.Domain.ValueObjects;
using Microsoft.Data.SqlClient;

namespace FileManager.Infrastructure.Persistence.SqlServer.Repositories;

public sealed class StorageFileRepository(ISqlConnectionFactory connectionFactory) : IStorageFileRepository
{
    private static readonly SqlColumn[] SelectAll =
    [
        StorageFiles.Id, StorageFiles.BusinessId, StorageFiles.ApplicationId, StorageFiles.ParentFolderId,
        StorageFiles.Name, StorageFiles.MimeType, StorageFiles.SizeBytes, StorageFiles.ContentHash,
        StorageFiles.ObjectKey, StorageFiles.ThumbnailObjectKey, StorageFiles.Provider, StorageFiles.FileType,
        StorageFiles.ThumbnailStatus, StorageFiles.ConversionStatus, StorageFiles.UploadStatus, StorageFiles.OcrStatus,
        StorageFiles.MetadataJson, StorageFiles.IsDeleted, StorageFiles.DeletionTime, StorageFiles.CreationTime,
        StorageFiles.CreatorId, StorageFiles.LastModificationTime, StorageFiles.LastModifierId
    ];

    public async Task<StorageFile?> GetAsync(long applicationId, long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {StorageFiles.Table}
            WHERE {StorageFiles.Id.Name} = {StorageFiles.Id.Parameter} AND {StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
            """;
        cmd.Add(StorageFiles.Id, id);
        cmd.Add(StorageFiles.ApplicationId, applicationId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<StorageFile?> GetByBusinessIdAsync(long applicationId, BusinessId businessId, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {StorageFiles.Table}
            WHERE {StorageFiles.BusinessId.Name} = {StorageFiles.BusinessId.Parameter} AND {StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
            """;
        cmd.Add(StorageFiles.BusinessId, (Guid)businessId);
        cmd.Add(StorageFiles.ApplicationId, applicationId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<StorageFile?> GetByContentHashAsync(long applicationId, ContentHash contentHash, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {StorageFiles.Table}
            WHERE {StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
              AND {StorageFiles.ContentHash.Name} = {StorageFiles.ContentHash.Parameter}
              AND {StorageFiles.IsDeleted.Name} = 0
              AND {StorageFiles.UploadStatus.Name} = {StorageFiles.UploadStatus.Parameter}
            """;
        cmd.Add(StorageFiles.ApplicationId, applicationId);
        cmd.Add(StorageFiles.ContentHash, contentHash.Value);
        cmd.Add(StorageFiles.UploadStatus, (int)UploadStatus.Completed);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> ExistsAsync(long applicationId, long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM {StorageFiles.Table}
            WHERE {StorageFiles.Id.Name} = {StorageFiles.Id.Parameter} AND {StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
            """;
        cmd.Add(StorageFiles.Id, id);
        cmd.Add(StorageFiles.ApplicationId, applicationId);

        return await cmd.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<bool> ExistsByNameAsync(long applicationId, long? parentFolderId, string name, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM {StorageFiles.Table}
            WHERE {StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
              AND {StorageFiles.Name.Name} = {StorageFiles.Name.Parameter}
              AND {StorageFiles.IsDeleted.Name} = 0
              AND {StorageFiles.UploadStatus.Name} <> {StorageFiles.UploadStatus.Parameter}
              AND (({StorageFiles.ParentFolderId.Name} IS NULL AND {StorageFiles.ParentFolderId.Parameter} IS NULL) OR {StorageFiles.ParentFolderId.Name} = {StorageFiles.ParentFolderId.Parameter})
            """;
        cmd.Add(StorageFiles.ApplicationId, applicationId);
        cmd.Add(StorageFiles.Name, name);
        cmd.Add(StorageFiles.UploadStatus, (int)UploadStatus.Failed);
        cmd.Add(StorageFiles.ParentFolderId, parentFolderId);

        return await cmd.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<IReadOnlyList<StorageFile>> GetByParentFolderIdAsync(
        long applicationId,
        long parentFolderId,
        bool? isDeleted,
        CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        var deletedFilter = isDeleted is null
            ? $"{StorageFiles.IsDeleted.Name} = 0"
            : $"{StorageFiles.IsDeleted.Name} = {StorageFiles.IsDeleted.Parameter}";

        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {StorageFiles.Table}
            WHERE {StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
              AND {StorageFiles.ParentFolderId.Name} = {StorageFiles.ParentFolderId.Parameter}
              AND {StorageFiles.UploadStatus.Name} <> {StorageFiles.UploadStatus.Parameter}
              AND {deletedFilter}
            ORDER BY {StorageFiles.Name.Name}
            """;
        cmd.Add(StorageFiles.ApplicationId, applicationId);
        cmd.Add(StorageFiles.ParentFolderId, parentFolderId);
        cmd.Add(StorageFiles.UploadStatus, (int)UploadStatus.Failed);
        if (isDeleted is not null)
            cmd.Add(StorageFiles.IsDeleted, isDeleted.Value);

        var files = new List<StorageFile>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            files.Add(Map(reader));

        return files;
    }

    public async Task InsertAsync(StorageFile file, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            INSERT INTO {StorageFiles.Table}
                ({StorageFiles.BusinessId.Name}, {StorageFiles.ApplicationId.Name}, {StorageFiles.ParentFolderId.Name},
                 {StorageFiles.Name.Name}, {StorageFiles.MimeType.Name}, {StorageFiles.SizeBytes.Name}, {StorageFiles.ContentHash.Name},
                 {StorageFiles.ObjectKey.Name}, {StorageFiles.ThumbnailObjectKey.Name}, {StorageFiles.Provider.Name}, {StorageFiles.FileType.Name},
                 {StorageFiles.ThumbnailStatus.Name}, {StorageFiles.ConversionStatus.Name}, {StorageFiles.UploadStatus.Name}, {StorageFiles.OcrStatus.Name},
                 {StorageFiles.MetadataJson.Name}, {StorageFiles.IsDeleted.Name}, {StorageFiles.CreationTime.Name}, {StorageFiles.CreatorId.Name})
            OUTPUT INSERTED.{StorageFiles.Id.Name}
            VALUES
                ({StorageFiles.BusinessId.Parameter}, {StorageFiles.ApplicationId.Parameter}, {StorageFiles.ParentFolderId.Parameter},
                 {StorageFiles.Name.Parameter}, {StorageFiles.MimeType.Parameter}, {StorageFiles.SizeBytes.Parameter}, {StorageFiles.ContentHash.Parameter},
                 {StorageFiles.ObjectKey.Parameter}, {StorageFiles.ThumbnailObjectKey.Parameter}, {StorageFiles.Provider.Parameter}, {StorageFiles.FileType.Parameter},
                 {StorageFiles.ThumbnailStatus.Parameter}, {StorageFiles.ConversionStatus.Parameter}, {StorageFiles.UploadStatus.Parameter}, {StorageFiles.OcrStatus.Parameter},
                 {StorageFiles.MetadataJson.Parameter}, {StorageFiles.IsDeleted.Parameter}, {StorageFiles.CreationTime.Parameter}, {StorageFiles.CreatorId.Parameter})
            """;

        cmd.Add(StorageFiles.BusinessId, (Guid)file.BusinessId);
        cmd.Add(StorageFiles.ApplicationId, file.ApplicationId);
        cmd.Add(StorageFiles.ParentFolderId, file.ParentFolderId);
        cmd.Add(StorageFiles.Name, file.Name.Value);
        cmd.Add(StorageFiles.MimeType, file.MimeType.Value);
        cmd.Add(StorageFiles.SizeBytes, file.Size.Bytes);
        cmd.Add(StorageFiles.ContentHash, file.ContentHash.Value);
        cmd.Add(StorageFiles.ObjectKey, (string)file.ObjectKey);
        cmd.Add(StorageFiles.ThumbnailObjectKey, file.ThumbnailObjectKey is null ? null : (string)file.ThumbnailObjectKey);
        cmd.Add(StorageFiles.Provider, (int)file.Provider);
        cmd.Add(StorageFiles.FileType, (int)file.FileType);
        cmd.Add(StorageFiles.ThumbnailStatus, (int)file.ThumbnailStatus);
        cmd.Add(StorageFiles.ConversionStatus, (int)file.ConversionStatus);
        cmd.Add(StorageFiles.UploadStatus, (int)file.UploadStatus);
        cmd.Add(StorageFiles.OcrStatus, (int)file.OcrStatus);
        cmd.Add(StorageFiles.MetadataJson, SerializeMetadata(file.Metadata));
        cmd.Add(StorageFiles.IsDeleted, file.IsDeleted);
        cmd.Add(StorageFiles.CreationTime, file.CreationTime);
        cmd.Add(StorageFiles.CreatorId, file.CreatorId);

        var generatedId = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        file.AssignId(generatedId);
    }

    public async Task UpdateAsync(StorageFile file, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            UPDATE {StorageFiles.Table}
            SET {SqlSchema.SetClause(
                StorageFiles.ParentFolderId, StorageFiles.Name, StorageFiles.MimeType, StorageFiles.SizeBytes,
                StorageFiles.ContentHash, StorageFiles.ObjectKey, StorageFiles.ThumbnailObjectKey, StorageFiles.FileType,
                StorageFiles.ThumbnailStatus, StorageFiles.ConversionStatus, StorageFiles.UploadStatus, StorageFiles.OcrStatus,
                StorageFiles.MetadataJson, StorageFiles.IsDeleted, StorageFiles.DeletionTime,
                StorageFiles.LastModificationTime, StorageFiles.LastModifierId)}
            WHERE {StorageFiles.Id.Name} = {StorageFiles.Id.Parameter} AND {StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
            """;

        cmd.Add(StorageFiles.ParentFolderId, file.ParentFolderId);
        cmd.Add(StorageFiles.Name, file.Name.Value);
        cmd.Add(StorageFiles.MimeType, file.MimeType.Value);
        cmd.Add(StorageFiles.SizeBytes, file.Size.Bytes);
        cmd.Add(StorageFiles.ContentHash, file.ContentHash.Value);
        cmd.Add(StorageFiles.ObjectKey, (string)file.ObjectKey);
        cmd.Add(StorageFiles.ThumbnailObjectKey, file.ThumbnailObjectKey is null ? null : (string)file.ThumbnailObjectKey);
        cmd.Add(StorageFiles.FileType, (int)file.FileType);
        cmd.Add(StorageFiles.ThumbnailStatus, (int)file.ThumbnailStatus);
        cmd.Add(StorageFiles.ConversionStatus, (int)file.ConversionStatus);
        cmd.Add(StorageFiles.UploadStatus, (int)file.UploadStatus);
        cmd.Add(StorageFiles.OcrStatus, (int)file.OcrStatus);
        cmd.Add(StorageFiles.MetadataJson, SerializeMetadata(file.Metadata));
        cmd.Add(StorageFiles.IsDeleted, file.IsDeleted);
        cmd.Add(StorageFiles.DeletionTime, file.DeletionTime);
        cmd.Add(StorageFiles.LastModificationTime, file.LastModificationTime);
        cmd.Add(StorageFiles.LastModifierId, file.LastModifierId);
        cmd.Add(StorageFiles.Id, file.Id);
        cmd.Add(StorageFiles.ApplicationId, file.ApplicationId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(long applicationId, long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            DELETE FROM {StorageFiles.Table}
            WHERE {StorageFiles.Id.Name} = {StorageFiles.Id.Parameter} AND {StorageFiles.ApplicationId.Name} = {StorageFiles.ApplicationId.Parameter}
            """;
        cmd.Add(StorageFiles.Id, id);
        cmd.Add(StorageFiles.ApplicationId, applicationId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string? SerializeMetadata(FileMetadata metadata) =>
        metadata.Values.Count == 0 ? null : JsonSerializer.Serialize(metadata.Values);

    private static FileMetadata DeserializeMetadata(string? json) =>
        string.IsNullOrEmpty(json)
            ? FileMetadata.Empty()
            : FileMetadata.FromDictionary(JsonSerializer.Deserialize<Dictionary<string, string>>(json));

    private static StorageFile Map(SqlDataReader reader) =>
        StorageFile.FromPersistence(
            id: reader.GetInt64(0),
            businessId: reader.GetGuid(1),
            applicationId: reader.GetInt64(2),
            parentFolderId: reader.NullableLong(3),
            name: FileName.FromString(reader.GetString(4)),
            mimeType: MimeType.FromString(reader.GetString(5)),
            size: FileSize.FromBytes(reader.GetInt64(6)),
            contentHash: ContentHash.FromString(reader.GetString(7)),
            objectKey: StorageObjectKey.FromString(reader.GetString(8)),
            thumbnailObjectKey: reader.NullableString(9) is { } key ? StorageObjectKey.FromString(key) : null,
            provider: (StorageProvider)reader.GetInt32(10),
            fileType: (StorageFileType)reader.GetInt32(11),
            thumbnailStatus: (ThumbnailStatus)reader.GetInt32(12),
            conversionStatus: (ConversionStatus)reader.GetInt32(13),
            uploadStatus: (UploadStatus)reader.GetInt32(14),
            ocrStatus: (OcrStatus)reader.GetInt32(15),
            metadata: DeserializeMetadata(reader.NullableString(16)),
            isDeleted: reader.GetBoolean(17),
            deletionTime: reader.NullableDateTime(18),
            creationTime: reader.GetDateTime(19),
            creatorId: reader.GetInt64(20),
            lastModificationTime: reader.NullableDateTime(21),
            lastModifierId: reader.NullableLong(22));
}
