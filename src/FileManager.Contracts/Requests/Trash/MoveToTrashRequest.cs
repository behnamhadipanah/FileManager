using FileManager.Contracts.Enumerations;

namespace FileManager.Contracts.Requests.Trash;

public sealed class MoveToTrashRequest
{
    public TrashItemType ItemType { get; set; }
    public long ItemId { get; set; }
}
