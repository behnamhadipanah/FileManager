using System.Data;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;

namespace FileManager.Infrastructure.Persistence.SqlServer.Tables;

internal static class RefreshTokens
{
    public const string Schema = SqlTableDefinitions.Schema;
    public const string TableName = "RefreshTokens";
    public const string Alias = "rt";

    public static string Table => SqlTableDefinitions.Qualify(TableName);
    public static string From => SqlTableDefinitions.From(TableName, Alias);

    public static readonly SqlColumn<Guid> Id = new("Id", SqlDbType.UniqueIdentifier);
    public static readonly SqlColumn<long> UserId = new("UserId", SqlDbType.BigInt);
    public static readonly SqlColumn<string> Token = new("Token", SqlDbType.NVarChar, 512);
    public static readonly SqlColumn<DateTime> Expires = new("Expires", SqlDbType.DateTime2);
    public static readonly SqlColumn<DateTime> Created = new("Created", SqlDbType.DateTime2);
    public static readonly SqlColumn<string?> CreatedByIp = new("CreatedByIp", SqlDbType.NVarChar, 64);
    public static readonly SqlColumn<DateTime?> Revoked = new("Revoked", SqlDbType.DateTime2);
    public static readonly SqlColumn<string?> RevokedByIp = new("RevokedByIp", SqlDbType.NVarChar, 64);
    public static readonly SqlColumn<string?> ReplacedByToken = new("ReplacedByToken", SqlDbType.NVarChar, 512);
}
