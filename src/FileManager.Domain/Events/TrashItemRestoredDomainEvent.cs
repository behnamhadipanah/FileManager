using FileManager.Domain.Enumerations;
using Kootam.Framework.Domain.Events;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Events;

public sealed record TrashItemRestoredDomainEvent(
    BusinessId BusinessId,
    long ApplicationId,
    TrashItemType ItemType,
    long ItemId) : IDomainEvent;
