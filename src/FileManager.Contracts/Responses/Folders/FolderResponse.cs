namespace FileManager.Contracts.Responses.Folders;

public sealed class FolderResponse
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderBusinessId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletionTime { get; set; }
    public DateTime CreationTime { get; set; }
}
