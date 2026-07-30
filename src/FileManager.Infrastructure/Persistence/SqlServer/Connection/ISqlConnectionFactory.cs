using System.Data;

namespace FileManager.Infrastructure.Persistence.SqlServer.Connection;

public interface ISqlConnectionFactory
{
    IDbConnection CreateWriteConnection();
    IDbConnection CreateReadConnection();
}
