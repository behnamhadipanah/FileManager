namespace FileManager.Infrastructure.Configuration;

public sealed class SqlServerOptions
{
    public const string SectionName = "SqlServer";

    /// <summary>
    /// When true, ensures the target database exists and applies pending SQL migrations on startup.
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Optional read-replica connection string. Falls back to <see cref="ConnectionString"/> when not set.
    /// </summary>
    public string? ReadConnectionString { get; set; }
}
