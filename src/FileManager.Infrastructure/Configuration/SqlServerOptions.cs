namespace FileManager.Infrastructure.Configuration;

public sealed class SqlServerOptions
{
    public const string SectionName = "SqlServer";

    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Optional read-replica connection string. Falls back to <see cref="ConnectionString"/> when not set.
    /// </summary>
    public string? ReadConnectionString { get; set; }
}
