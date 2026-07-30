using Kootam.Framework.Domain.Events;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Events;

public sealed record FolderCreatedDomainEvent(
    BusinessId BusinessId,
    long ApplicationId,
    string Name,
    long? ParentFolderId) : IDomainEvent;
