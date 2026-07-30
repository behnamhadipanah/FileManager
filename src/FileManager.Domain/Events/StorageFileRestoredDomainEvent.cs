using Kootam.Framework.Domain.Events;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Events;

public sealed record StorageFileRestoredDomainEvent(
    BusinessId BusinessId,
    long ApplicationId,
    long FileId,
    string Name,
    long? ParentFolderId) : IDomainEvent;
