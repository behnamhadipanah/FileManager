using System.Reflection;
using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace FileManager.Infrastructure.Persistence.SqlServer.Migrations;

/// <summary>
/// Minimal, ORM-free migration runner. Executes the embedded *.sql scripts in this
/// folder in filename order, tracking what already ran in a __SchemaMigrations table.
/// </summary>
public sealed class SqlMigrationRunner(ISqlConnectionFactory connectionFactory, ILogger<SqlMigrationRunner> logger)
{
    private const string HistoryTable = "__SchemaMigrations";

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await EnsureHistoryTableAsync(conn, cancellationToken);
        var applied = await GetAppliedMigrationsAsync(conn, cancellationToken);

        foreach (var (name, script) in GetEmbeddedScripts())
        {
            if (applied.Contains(name))
                continue;

            logger.LogInformation("Applying SQL migration {MigrationName}", name);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = script;
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var insert = conn.CreateCommand())
            {
                insert.CommandText = $"INSERT INTO {HistoryTable} (Name, AppliedAt) VALUES (@Name, @AppliedAt)";
                insert.Parameters.AddWithValue("@Name", name);
                insert.Parameters.AddWithValue("@AppliedAt", DateTime.UtcNow);
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }

    private static async Task EnsureHistoryTableAsync(SqlConnection conn, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '{HistoryTable}')
            BEGIN
                CREATE TABLE {HistoryTable}
                (
                    Name      NVARCHAR(260) NOT NULL CONSTRAINT PK_{HistoryTable} PRIMARY KEY,
                    AppliedAt DATETIME2     NOT NULL
                );
            END
            """;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<HashSet<string>> GetAppliedMigrationsAsync(SqlConnection conn, CancellationToken cancellationToken)
    {
        var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Name FROM {HistoryTable}";

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            applied.Add(reader.GetString(0));

        return applied;
    }

    private static IEnumerable<(string Name, string Script)> GetEmbeddedScripts()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.Contains(".Persistence.SqlServer.Migrations.", StringComparison.Ordinal)
                        && n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase);

        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var name = resourceName.Split('.').Reverse().Skip(1).First() + ".sql";
            yield return (name, reader.ReadToEnd());
        }
    }
}
