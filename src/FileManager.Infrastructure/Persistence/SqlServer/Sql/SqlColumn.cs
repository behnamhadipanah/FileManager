using System.Data;

namespace FileManager.Infrastructure.Persistence.SqlServer.Sql;

public abstract class SqlColumn
{
    public string Name { get; }
    public SqlDbType DbType { get; }
    public int? Size { get; }

    protected SqlColumn(string name, SqlDbType dbType, int? size = null)
    {
        Name = name;
        DbType = dbType;
        Size = size;
    }

    public string Parameter => "@" + Name;
}

public sealed class SqlColumn<T> : SqlColumn
{
    public SqlColumn(string name, SqlDbType dbType, int? size = null) : base(name, dbType, size)
    {
    }
}
