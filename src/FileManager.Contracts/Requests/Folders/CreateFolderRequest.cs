namespace FileManager.Contracts.Requests.Folders;

public sealed class CreateFolderRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderBusinessId { get; set; }
}
