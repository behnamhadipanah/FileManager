using FileManager.Domain.Common;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Events;
using FileManager.Domain.Messages;
using Kootam.Framework.Domain.Contracts.Capability;
using Kootam.Framework.Domain.Contracts.Markers;
using Kootam.Framework.Domain.Entities;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Aggregates.TrashAgg;

public sealed class TrashItem : AggregateRoot, IMultiTenant<long>, IMultiTenantEntity,
    ICreationAuditedObject<long>
{
    public long ApplicationId { get; private set; }
    public long TenantId => ApplicationId;
    public TrashItemType ItemType { get; private set; }
    public long ItemId { get; private set; }
    public string ItemName { get; private set; } = null!;
    public long? OriginalParentFolderId { get; private set; }
    public bool IsRestored { get; private set; }
    public bool IsPurged { get; private set; }
    public DateTime? PurgedTime { get; private set; }
    public DateTime CreationTime { get; private set; }
    public long CreatorId { get; private set; }
    public DateTime? LastModificationTime { get; private set; }
    public long? LastModifierId { get; private set; }

    private TrashItem() { }

    public static TrashItem Create(
        long applicationId,
        TrashItemType itemType,
        long itemId,
        string itemName,
        long? originalParentFolderId,
        DateTime now,
        long creatorId = 0)
    {
        Guard.Positive(applicationId, nameof(applicationId));
        Guard.NotEmpty(itemName, nameof(itemName));

        var trashItem = new TrashItem
        {
            ApplicationId = applicationId,
            ItemType = itemType,
            ItemId = itemId,
            ItemName = itemName.Trim(),
            OriginalParentFolderId = originalParentFolderId
        };

        trashItem.SetCreated(now);
        trashItem.SetCreator(creatorId);
        trashItem.Apply(new TrashItemCreatedDomainEvent(
            trashItem.BusinessId, applicationId, itemType, itemId, trashItem.ItemName));

        return trashItem;
    }

    /// <summary>
    /// Rehydrates an aggregate from persisted state without raising domain events.
    /// Used exclusively by Infrastructure repositories (see AssemblyInfo.cs InternalsVisibleTo).
    /// </summary>
    internal static TrashItem FromPersistence(
        long id,
        Guid businessId,
        long applicationId,
        TrashItemType itemType,
        long itemId,
        string itemName,
        long? originalParentFolderId,
        bool isRestored,
        bool isPurged,
        DateTime? purgedTime,
        DateTime creationTime,
        long creatorId,
        DateTime? lastModificationTime,
        long? lastModifierId) =>
        new()
        {
            Id = id,
            BusinessId = BusinessId.FromGuid(businessId),
            ApplicationId = applicationId,
            ItemType = itemType,
            ItemId = itemId,
            ItemName = itemName,
            OriginalParentFolderId = originalParentFolderId,
            IsRestored = isRestored,
            IsPurged = isPurged,
            PurgedTime = purgedTime,
            CreationTime = creationTime,
            CreatorId = creatorId,
            LastModificationTime = lastModificationTime,
            LastModifierId = lastModifierId
        };

    public void Restore(DateTime now, long modifierId = 0)
    {
        EnsureNotPurged();
        if (IsRestored)
            return;

        IsRestored = true;
        SetLastModification(now);
        SetLastModifier(modifierId);
        Apply(new TrashItemRestoredDomainEvent(BusinessId, ApplicationId, ItemType, ItemId));
    }

    public void Purge(DateTime now, long modifierId = 0)
    {
        EnsureNotPurged();

        IsPurged = true;
        PurgedTime = now;
        SetLastModification(now);
        SetLastModifier(modifierId);
        Apply(new TrashItemPurgedDomainEvent(BusinessId, ApplicationId, ItemType, ItemId));
    }

    public void SetCreated(DateTime now) => CreationTime = now;
    public void SetCreator(long creatorId) => CreatorId = creatorId;
    public void SetLastModification(DateTime dateTime) => LastModificationTime = dateTime;
    public void SetLastModifier(long userId) => LastModifierId = userId;

    /// <summary>
    /// Assigns the database-generated identity after insert. No ORM is used, so
    /// repositories in the Infrastructure layer must push the generated Id back
    /// onto the aggregate themselves (see AssemblyInfo.cs InternalsVisibleTo).
    /// </summary>
    internal void AssignId(long id) => Id = id;

    private void EnsureNotPurged()
    {
        if (IsPurged)
            throw new Exceptions.DomainException(DomainMessages.TrashItemAlreadyPurged);
    }

    private void Apply(TrashItemCreatedDomainEvent @event) => AddEvent(@event);
    private void Apply(TrashItemRestoredDomainEvent @event) => AddEvent(@event);
    private void Apply(TrashItemPurgedDomainEvent @event) => AddEvent(@event);
}
