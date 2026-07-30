using Kootam.Framework.Domain.Events;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Events;

public sealed record FolderRestoredDomainEvent(
    BusinessId BusinessId,
    long ApplicationId,
    long FolderId,
    string Name,
    long? ParentFolderId) : IDomainEvent;
