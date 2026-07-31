namespace FileManager.Infrastructure.Persistence.SqlServer.Sql;

public static class SqlSchema
{
    public static string Cols(params SqlColumn[] columns) =>
        string.Join(", ", columns.Select(c => c.Name));

    public static string ColsWithAlias(string alias, params SqlColumn[] columns) =>
        string.Join(", ", columns.Select(c => $"{alias}.{c.Name}"));

    public static string SetClause(params SqlColumn[] columns) =>
        string.Join(", ", columns.Select(c => $"{c.Name} = {c.Parameter}"));

    public static string SetClauseWithAlias(string alias, params SqlColumn[] columns) =>
        string.Join(", ", columns.Select(c => $"{alias}.{c.Name} = {c.Parameter}"));
}
