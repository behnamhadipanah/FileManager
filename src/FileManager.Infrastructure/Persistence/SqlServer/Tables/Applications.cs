using System.Data;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;

namespace FileManager.Infrastructure.Persistence.SqlServer.Tables;

internal static class Applications
{
    public const string Table = "Applications";

    public static readonly SqlColumn<long> Id = new("Id", SqlDbType.BigInt);
    public static readonly SqlColumn<Guid> BusinessId = new("BusinessId", SqlDbType.UniqueIdentifier);
    public static readonly SqlColumn<string> ApplicationName = new("ApplicationName", SqlDbType.NVarChar, 200);
    public static readonly SqlColumn<string> Token = new("Token", SqlDbType.NVarChar, 256);
    public static readonly SqlColumn<long> MinImageSize = new("MinImageSize", SqlDbType.BigInt);
    public static readonly SqlColumn<long> MaxImageSize = new("MaxImageSize", SqlDbType.BigInt);
    public static readonly SqlColumn<long> MinVideoSize = new("MinVideoSize", SqlDbType.BigInt);
    public static readonly SqlColumn<long> MaxVideoSize = new("MaxVideoSize", SqlDbType.BigInt);
    public static readonly SqlColumn<long> MinDocumentSize = new("MinDocumentSize", SqlDbType.BigInt);
    public static readonly SqlColumn<long> MaxDocumentSize = new("MaxDocumentSize", SqlDbType.BigInt);
    public static readonly SqlColumn<bool> IsActive = new("IsActive", SqlDbType.Bit);
    public static readonly SqlColumn<DateTime> CreationTime = new("CreationTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<long> CreatorId = new("CreatorId", SqlDbType.BigInt);
    public static readonly SqlColumn<DateTime?> LastModificationTime = new("LastModificationTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<long?> LastModifierId = new("LastModifierId", SqlDbType.BigInt);
}
