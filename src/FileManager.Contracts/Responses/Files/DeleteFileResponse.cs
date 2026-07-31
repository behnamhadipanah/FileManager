namespace FileManager.Contracts.Responses.Files;

public sealed class DeleteFileResponse
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long TrashItemId { get; set; }
}
