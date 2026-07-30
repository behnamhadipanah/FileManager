using Microsoft.Data.SqlClient;

namespace FileManager.Infrastructure.Persistence.SqlServer.Sql;

public static class SqlCommandExtensions
{
    public static void Add<T>(this SqlCommand command, SqlColumn<T> column, T value)
    {
        var parameter = new SqlParameter(column.Parameter, column.DbType);
        if (column.Size.HasValue)
            parameter.Size = column.Size.Value;

        parameter.Value = value is null ? DBNull.Value : (object)value;
        command.Parameters.Add(parameter);
    }
}
