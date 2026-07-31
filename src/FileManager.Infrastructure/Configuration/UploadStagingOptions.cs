namespace FileManager.Infrastructure.Configuration;

public sealed class UploadStagingOptions
{
    public const string SectionName = "UploadStaging";

    public string Path { get; set; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "filemanager-uploads");
}
