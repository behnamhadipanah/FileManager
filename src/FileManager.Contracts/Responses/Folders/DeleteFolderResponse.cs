namespace FileManager.Contracts.Responses.Folders;

public sealed class DeleteFolderResponse
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long TrashItemId { get; set; }
}
