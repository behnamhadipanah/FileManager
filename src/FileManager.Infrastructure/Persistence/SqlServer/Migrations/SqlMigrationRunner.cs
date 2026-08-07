using System.Reflection;
using FileManager.Infrastructure.Configuration;
using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using FileManager.Infrastructure.Persistence.SqlServer.Tables;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileManager.Infrastructure.Persistence.SqlServer.Migrations;

/// <summary>
/// Minimal, ORM-free migration runner. Executes the embedded *.sql scripts in this
/// folder in filename order, tracking what already ran in a schema-qualified __SchemaMigrations table.
/// </summary>
public sealed class SqlMigrationRunner(
    ISqlConnectionFactory connectionFactory,
    IOptions<SqlServerOptions> options,
    ILogger<SqlMigrationRunner> logger)
{
    private readonly SqlServerOptions _options = options.Value;

    private static string Schema => SqlTableDefinitions.Schema;

    private static string HistoryTable =>
        SqlTableDefinitions.Qualify(Schema, SqlTableDefinitions.MigrationsTableName);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        
        await EnsureDatabaseExistsAsync(cancellationToken);

        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        
        await conn.OpenAsync(cancellationToken);

        await EnsureSchemaExistsAsync(conn, cancellationToken);
        await EnsureHistoryTableAsync(conn, cancellationToken);
        var applied = await GetAppliedMigrationsAsync(conn, cancellationToken);

        foreach (var (name, script) in GetEmbeddedScripts())
        {
            if (applied.Contains(name))
                continue;

            logger.LogInformation("Applying SQL migration {MigrationName}", name);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = ApplyTokens(script);
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

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {

        var builder = new SqlConnectionStringBuilder(_options.ConnectionString);
        
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            logger.LogWarning("SqlServer connection string has no database name; skipping database creation.");
            return;
        }

        builder.InitialCatalog = "master";

        await using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        var escapedName = databaseName.Replace("]", "]]", StringComparison.Ordinal);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = @DatabaseName)
            BEGIN
                CREATE DATABASE [{escapedName}];
            END
            """;
        cmd.Parameters.AddWithValue("@DatabaseName", databaseName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        logger.LogInformation("Ensured SQL Server database {DatabaseName} exists", databaseName);
    }

    private static async Task EnsureSchemaExistsAsync(SqlConnection conn, CancellationToken cancellationToken)
    {
        var escapedSchema = Schema.Replace("]", "]]", StringComparison.Ordinal);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = @Schema)
                EXEC(N'CREATE SCHEMA [{escapedSchema}]');
            """;
        cmd.Parameters.AddWithValue("@Schema", Schema);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureHistoryTableAsync(SqlConnection conn, CancellationToken cancellationToken)
    {
        var escapedSchema = Schema.Replace("]", "]]", StringComparison.Ordinal);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            IF NOT EXISTS (
                SELECT 1
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = @Schema AND t.name = @TableName)
            BEGIN
                CREATE TABLE [{escapedSchema}].[{SqlTableDefinitions.MigrationsTableName}]
                (
                    Name      NVARCHAR(260) NOT NULL CONSTRAINT PK_{SqlTableDefinitions.MigrationsTableName} PRIMARY KEY,
                    AppliedAt DATETIME2     NOT NULL
                );
            END
            """;
        cmd.Parameters.AddWithValue("@Schema", Schema);
        cmd.Parameters.AddWithValue("@TableName", SqlTableDefinitions.MigrationsTableName);
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

    private static string ApplyTokens(string script) =>
        script.Replace("{{Schema}}", Schema, StringComparison.Ordinal);
}
