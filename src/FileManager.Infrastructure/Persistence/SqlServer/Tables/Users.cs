using System.Data;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;

namespace FileManager.Infrastructure.Persistence.SqlServer.Tables;

internal static class Users
{
    public const string Schema = SqlTableDefinitions.Schema;
    public const string TableName = "Users";
    public const string Alias = "u";

    public static string Table => SqlTableDefinitions.Qualify(TableName);
    public static string From => SqlTableDefinitions.From(TableName, Alias);

    public static readonly SqlColumn<long> Id = new("Id", SqlDbType.BigInt);
    public static readonly SqlColumn<string> FirstName = new("FirstName", SqlDbType.NVarChar, 100);
    public static readonly SqlColumn<string> LastName = new("LastName", SqlDbType.NVarChar, 100);
    public static readonly SqlColumn<string> Email = new("Email", SqlDbType.NVarChar, 256);
    public static readonly SqlColumn<string> PasswordHash = new("PasswordHash", SqlDbType.NVarChar, 512);
    public static readonly SqlColumn<bool> IsActive = new("IsActive", SqlDbType.Bit);
    public static readonly SqlColumn<DateTime> CreationTime = new("CreationTime", SqlDbType.DateTime2);
}
