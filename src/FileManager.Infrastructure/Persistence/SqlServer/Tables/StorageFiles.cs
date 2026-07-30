using System.Data;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;

namespace FileManager.Infrastructure.Persistence.SqlServer.Tables;

internal static class StorageFiles
{
    public const string Table = "StorageFiles";

    public static readonly SqlColumn<long> Id = new("Id", SqlDbType.BigInt);
    public static readonly SqlColumn<Guid> BusinessId = new("BusinessId", SqlDbType.UniqueIdentifier);
    public static readonly SqlColumn<long> ApplicationId = new("ApplicationId", SqlDbType.BigInt);
    public static readonly SqlColumn<long?> ParentFolderId = new("ParentFolderId", SqlDbType.BigInt);
    public static readonly SqlColumn<string> Name = new("Name", SqlDbType.NVarChar, 255);
    public static readonly SqlColumn<string> MimeType = new("MimeType", SqlDbType.NVarChar, 200);
    public static readonly SqlColumn<long> SizeBytes = new("SizeBytes", SqlDbType.BigInt);
    public static readonly SqlColumn<string> ContentHash = new("ContentHash", SqlDbType.NVarChar, 128);
    public static readonly SqlColumn<string> ObjectKey = new("ObjectKey", SqlDbType.NVarChar, 500);
    public static readonly SqlColumn<string?> ThumbnailObjectKey = new("ThumbnailObjectKey", SqlDbType.NVarChar, 500);
    public static readonly SqlColumn<int> Provider = new("Provider", SqlDbType.Int);
    public static readonly SqlColumn<int> FileType = new("FileType", SqlDbType.Int);
    public static readonly SqlColumn<int> ThumbnailStatus = new("ThumbnailStatus", SqlDbType.Int);
    public static readonly SqlColumn<int> ConversionStatus = new("ConversionStatus", SqlDbType.Int);
    public static readonly SqlColumn<int> OcrStatus = new("OcrStatus", SqlDbType.Int);
    public static readonly SqlColumn<string?> MetadataJson = new("MetadataJson", SqlDbType.NVarChar, -1);
    public static readonly SqlColumn<bool> IsDeleted = new("IsDeleted", SqlDbType.Bit);
    public static readonly SqlColumn<DateTime?> DeletionTime = new("DeletionTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<DateTime> CreationTime = new("CreationTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<long> CreatorId = new("CreatorId", SqlDbType.BigInt);
    public static readonly SqlColumn<DateTime?> LastModificationTime = new("LastModificationTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<long?> LastModifierId = new("LastModifierId", SqlDbType.BigInt);
}
