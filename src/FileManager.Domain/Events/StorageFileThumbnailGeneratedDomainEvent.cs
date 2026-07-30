using Kootam.Framework.Domain.Events;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Events;

public sealed record StorageFileThumbnailGeneratedDomainEvent(
    BusinessId BusinessId,
    long ApplicationId,
    long FileId,
    string ThumbnailObjectKey) : IDomainEvent;
