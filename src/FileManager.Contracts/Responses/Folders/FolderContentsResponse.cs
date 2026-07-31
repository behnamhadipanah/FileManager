using FileManager.Contracts.Responses.Files;
using FileManager.Contracts.Responses.Folders;

namespace FileManager.Contracts.Responses.Folders;

public sealed class FolderContentsResponse
{
    public FolderResponse CurrentFolder { get; set; } = new();
    public IReadOnlyList<FolderResponse> Folders { get; set; } = [];
    public IReadOnlyList<StorageFileResponse> Files { get; set; } = [];
}
