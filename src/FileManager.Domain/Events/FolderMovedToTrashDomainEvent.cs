using Kootam.Framework.Domain.Events;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Events;

public sealed record FolderMovedToTrashDomainEvent(
    BusinessId BusinessId,
    long ApplicationId,
    long FolderId,
    string Name,
    long? ParentFolderId,
    DateTime DeletedAt) : IDomainEvent;
