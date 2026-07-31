namespace FileManager.Contracts.Responses.Files;

public sealed class StorageFileResponse
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public Guid? ParentFolderBusinessId { get; set; }
    public int FileType { get; set; }
    public int ConversionStatus { get; set; }
    public int UploadStatus { get; set; }
    public int ThumbnailStatus { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletionTime { get; set; }
    public DateTime CreationTime { get; set; }
}
