using System.Data;
using FileManager.Infrastructure.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FileManager.Infrastructure.Persistence.SqlServer.Connection;

public sealed class SqlConnectionFactory(IOptions<SqlServerOptions> options) : ISqlConnectionFactory
{
    private readonly SqlServerOptions _options = options.Value;

    public IDbConnection CreateWriteConnection() => new SqlConnection(_options.ConnectionString);

    public IDbConnection CreateReadConnection() =>
        new SqlConnection(string.IsNullOrWhiteSpace(_options.ReadConnectionString)
            ? _options.ConnectionString
            : _options.ReadConnectionString);
}
