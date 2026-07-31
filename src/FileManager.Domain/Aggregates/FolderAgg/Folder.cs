using FileManager.Domain.Common;
using FileManager.Domain.Events;
using FileManager.Domain.Messages;
using FileManager.Domain.ValueObjects;
using Kootam.Framework.Domain.Contracts.Capability;
using Kootam.Framework.Domain.Contracts.Markers;
using Kootam.Framework.Domain.Entities;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Aggregates.FolderAgg;

public sealed class Folder : AggregateRoot, IMultiTenant<long>, IMultiTenantEntity,
    ICreationAuditedObject<long>, ISoftDelete
{
    public long ApplicationId { get; private set; }
    public long TenantId => ApplicationId;
    public long? ParentFolderId { get; private set; }
    public FileName Name { get; private set; } = null!;
    public bool IsDeleted { get; private set; }
    public DateTime? DeletionTime { get; private set; }
    public DateTime CreationTime { get; private set; }
    public long CreatorId { get; private set; }
    public DateTime? LastModificationTime { get; private set; }
    public long? LastModifierId { get; private set; }

    

    private Folder() { }

    public const string RootFolderName = "Root";

    public static Folder CreateRoot(long applicationId, DateTime now, long creatorId = 0) =>
        Create(applicationId, FileName.FromString(RootFolderName), parentFolderId: null, now, creatorId);

    public static Folder Create(
        long applicationId,
        FileName name,
        long? parentFolderId,
        DateTime now,
        long creatorId = 0)
    {
        Guard.Positive(applicationId, nameof(applicationId));

        var folder = new Folder
        {
            ApplicationId = applicationId,
            Name = name,
            ParentFolderId = parentFolderId
        };

        folder.SetCreated(now);
        folder.SetCreator(creatorId);
        folder.Apply(new FolderCreatedDomainEvent(folder.BusinessId, applicationId, name.Value, parentFolderId));

        return folder;
    }

    /// <summary>
    /// Rehydrates an aggregate from persisted state without raising domain events.
    /// Used exclusively by Infrastructure repositories (see AssemblyInfo.cs InternalsVisibleTo).
    /// </summary>
    internal static Folder FromPersistence(
        long id,
        Guid businessId,
        long applicationId,
        long? parentFolderId,
        FileName name,
        bool isDeleted,
        DateTime? deletionTime,
        DateTime creationTime,
        long creatorId,
        DateTime? lastModificationTime,
        long? lastModifierId) =>
        new()
        {
            Id = id,
            BusinessId = BusinessId.FromGuid(businessId),
            ApplicationId = applicationId,
            ParentFolderId = parentFolderId,
            Name = name,
            IsDeleted = isDeleted,
            DeletionTime = deletionTime,
            CreationTime = creationTime,
            CreatorId = creatorId,
            LastModificationTime = lastModificationTime,
            LastModifierId = lastModifierId
        };

    public void Rename(FileName newName, DateTime now, long modifierId = 0)
    {
        EnsureNotDeleted();
        Name = newName;
        SetLastModification(now);
        SetLastModifier(modifierId);
    }

    public void Move(long? newParentFolderId, DateTime now, long modifierId = 0)
    {
        EnsureNotDeleted();

        if (newParentFolderId == Id)
            throw new Exceptions.DomainException(DomainMessages.CircularFolderReference);

        ParentFolderId = newParentFolderId;
        SetLastModification(now);
        SetLastModifier(modifierId);
    }

    public void Delete(DateTime now)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletionTime = now;
        Apply(new FolderMovedToTrashDomainEvent(BusinessId, ApplicationId, Id, Name.Value, ParentFolderId, now));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        DeletionTime = null;
        Apply(new FolderRestoredDomainEvent(BusinessId, ApplicationId, Id, Name.Value, ParentFolderId));
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

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new Exceptions.DomainException(DomainMessages.FileAlreadyDeleted);
    }

    private void Apply(FolderCreatedDomainEvent @event) => AddEvent(@event);
    private void Apply(FolderMovedToTrashDomainEvent @event) => AddEvent(@event);
    private void Apply(FolderRestoredDomainEvent @event) => AddEvent(@event);
}
