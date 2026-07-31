namespace FileManager.Contracts.Responses.Files;

public sealed class RestoreFileResponse
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? RestoredToFolderId { get; set; }
}
