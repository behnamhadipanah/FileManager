using System.Data;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;

namespace FileManager.Infrastructure.Persistence.SqlServer.Tables;

internal static class Folders
{
    public const string Table = "Folders";

    public static readonly SqlColumn<long> Id = new("Id", SqlDbType.BigInt);
    public static readonly SqlColumn<Guid> BusinessId = new("BusinessId", SqlDbType.UniqueIdentifier);
    public static readonly SqlColumn<long> ApplicationId = new("ApplicationId", SqlDbType.BigInt);
    public static readonly SqlColumn<long?> ParentFolderId = new("ParentFolderId", SqlDbType.BigInt);
    public static readonly SqlColumn<string> Name = new("Name", SqlDbType.NVarChar, 255);
    public static readonly SqlColumn<bool> IsDeleted = new("IsDeleted", SqlDbType.Bit);
    public static readonly SqlColumn<DateTime?> DeletionTime = new("DeletionTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<DateTime> CreationTime = new("CreationTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<long> CreatorId = new("CreatorId", SqlDbType.BigInt);
    public static readonly SqlColumn<DateTime?> LastModificationTime = new("LastModificationTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<long?> LastModifierId = new("LastModifierId", SqlDbType.BigInt);
}
