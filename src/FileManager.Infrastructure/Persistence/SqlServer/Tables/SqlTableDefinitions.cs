namespace FileManager.Infrastructure.Persistence.SqlServer.Tables;

internal static class SqlTableDefinitions
{
    public const string Schema = "fm";
    public const string MigrationsTableName = "__SchemaMigrations";

    public static string Qualify(string tableName) => Qualify(Schema, tableName);

    public static string Qualify(string schema, string tableName) =>
        $"[{schema}].[{tableName}]";

    public static string From(string tableName, string alias) =>
        $"{Qualify(tableName)} AS {alias}";
}
