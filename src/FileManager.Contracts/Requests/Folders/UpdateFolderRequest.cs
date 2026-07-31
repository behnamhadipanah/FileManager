namespace FileManager.Contracts.Requests.Folders;

public sealed class UpdateFolderRequest
{
    public Guid? ParentFolderBusinessId { get; set; }
}
