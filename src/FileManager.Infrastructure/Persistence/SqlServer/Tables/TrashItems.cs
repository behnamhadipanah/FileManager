using System.Data;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;

namespace FileManager.Infrastructure.Persistence.SqlServer.Tables;

internal static class TrashItems
{
    public const string Schema = SqlTableDefinitions.Schema;
    public const string TableName = "TrashItems";
    public const string Alias = "ti";

    public static string Table => SqlTableDefinitions.Qualify(TableName);
    public static string From => SqlTableDefinitions.From(TableName, Alias);

    public static readonly SqlColumn<long> Id = new("Id", SqlDbType.BigInt);
    public static readonly SqlColumn<Guid> BusinessId = new("BusinessId", SqlDbType.UniqueIdentifier);
    public static readonly SqlColumn<long> ApplicationId = new("ApplicationId", SqlDbType.BigInt);
    public static readonly SqlColumn<int> ItemType = new("ItemType", SqlDbType.Int);
    public static readonly SqlColumn<long> ItemId = new("ItemId", SqlDbType.BigInt);
    public static readonly SqlColumn<string> ItemName = new("ItemName", SqlDbType.NVarChar, 255);
    public static readonly SqlColumn<long?> OriginalParentFolderId = new("OriginalParentFolderId", SqlDbType.BigInt);
    public static readonly SqlColumn<bool> IsRestored = new("IsRestored", SqlDbType.Bit);
    public static readonly SqlColumn<bool> IsPurged = new("IsPurged", SqlDbType.Bit);
    public static readonly SqlColumn<DateTime?> PurgedTime = new("PurgedTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<DateTime> CreationTime = new("CreationTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<long> CreatorId = new("CreatorId", SqlDbType.BigInt);
    public static readonly SqlColumn<DateTime?> LastModificationTime = new("LastModificationTime", SqlDbType.DateTime2);
    public static readonly SqlColumn<long?> LastModifierId = new("LastModifierId", SqlDbType.BigInt);
}
