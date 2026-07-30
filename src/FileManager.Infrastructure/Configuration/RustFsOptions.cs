namespace FileManager.Infrastructure.Configuration;

public sealed class RustFsOptions
{
    public const string SectionName = "RustFs";

    public string ServiceUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool ForcePathStyle { get; set; } = true;

    /// <summary>Bucket that stores the original uploaded files.</summary>
    public string FilesBucket { get; set; } = "files";

    /// <summary>Separate bucket for generated thumbnails (requirement: thumbnails live in another bucket).</summary>
    public string ThumbnailsBucket { get; set; } = "thumbnails";
}
