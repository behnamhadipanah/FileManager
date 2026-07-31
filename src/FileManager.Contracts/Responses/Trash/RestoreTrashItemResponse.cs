using FileManager.Contracts.Enumerations;

namespace FileManager.Contracts.Responses.Trash;

public sealed class RestoreTrashItemResponse
{
    public TrashItemType ItemType { get; set; }
    public long ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public long? RestoredToFolderId { get; set; }
}
