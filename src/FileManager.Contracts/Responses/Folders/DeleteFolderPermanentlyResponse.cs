namespace FileManager.Contracts.Responses.Folders;

public sealed class DeleteFolderPermanentlyResponse
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
}
