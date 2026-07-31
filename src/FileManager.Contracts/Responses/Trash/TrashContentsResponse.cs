using FileManager.Contracts.Responses.Files;
using FileManager.Contracts.Responses.Folders;

namespace FileManager.Contracts.Responses.Trash;

public sealed class TrashContentsResponse
{
    public IReadOnlyList<TrashFolderItemResponse> Folders { get; set; } = [];
    public IReadOnlyList<TrashFileItemResponse> Files { get; set; } = [];
}

public sealed class TrashFolderItemResponse
{
    public long TrashItemId { get; set; }
    public FolderResponse Folder { get; set; } = new();
    public Guid? OriginalParentFolderBusinessId { get; set; }
    public DateTime TrashedTime { get; set; }
}

public sealed class TrashFileItemResponse
{
    public long TrashItemId { get; set; }
    public StorageFileResponse File { get; set; } = new();
    public Guid? OriginalParentFolderBusinessId { get; set; }
    public DateTime TrashedTime { get; set; }
}
