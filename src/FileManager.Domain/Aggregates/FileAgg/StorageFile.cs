using FileManager.Domain.Common;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Events;
using FileManager.Domain.Messages;
using FileManager.Domain.ValueObjects;
using Kootam.Framework.Domain.Contracts.Capability;
using Kootam.Framework.Domain.Contracts.Markers;
using Kootam.Framework.Domain.Entities;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Aggregates.FileAgg;

public sealed class StorageFile : AggregateRoot, IMultiTenant<long>, IMultiTenantEntity,
    ICreationAuditedObject<long>, ISoftDelete
{
    public long ApplicationId { get; private set; }
    public long TenantId => ApplicationId;
    public long? ParentFolderId { get; private set; }
    public FileName Name { get; private set; } = null!;
    public MimeType MimeType { get; private set; } = null!;
    public FileSize Size { get; private set; } = null!;
    public ContentHash ContentHash { get; private set; } = null!;
    public StorageObjectKey ObjectKey { get; private set; } = null!;
    public StorageObjectKey? ThumbnailObjectKey { get; private set; }
    public StorageProvider Provider { get; private set; }
    public StorageFileType FileType { get; private set; }
    public ThumbnailStatus ThumbnailStatus { get; private set; } = ThumbnailStatus.None;
    public ConversionStatus ConversionStatus { get; private set; } = ConversionStatus.None;
    public UploadStatus UploadStatus { get; private set; } = UploadStatus.Completed;
    public OcrStatus OcrStatus { get; private set; } = OcrStatus.None;
    public FileMetadata Metadata { get; private set; } = FileMetadata.Empty();
    public bool IsDeleted { get; private set; }
    public DateTime? DeletionTime { get; private set; }
    public DateTime CreationTime { get; private set; }
    public long CreatorId { get; private set; }
    public DateTime? LastModificationTime { get; private set; }
    public long? LastModifierId { get; private set; }

    private StorageFile() { }

    public static StorageFile Create(
        long applicationId,
        long? parentFolderId,
        FileName name,
        MimeType mimeType,
        FileSize size,
        ContentHash contentHash,
        StorageObjectKey objectKey,
        StorageProvider provider,
        StorageFileType fileType,
        FileMetadata metadata,
        DateTime now,
        long creatorId = 0)
    {
        Guard.Positive(applicationId, nameof(applicationId));

        var file = new StorageFile
        {
            ApplicationId = applicationId,
            ParentFolderId = parentFolderId,
            Name = name,
            MimeType = mimeType,
            Size = size,
            ContentHash = contentHash,
            ObjectKey = objectKey,
            Provider = provider,
            FileType = fileType,
            Metadata = metadata,
            ThumbnailStatus = fileType is StorageFileType.Image or StorageFileType.Video
                ? ThumbnailStatus.Pending
                : ThumbnailStatus.None
        };

        file.SetCreated(now);
        file.SetCreator(creatorId);
        file.Apply(new StorageFileUploadedDomainEvent(
            file.BusinessId, applicationId, name.Value, mimeType.Value, size.Bytes, fileType, parentFolderId));

        return file;
    }

    public static StorageFile CreatePendingUpload(
        long applicationId,
        long? parentFolderId,
        FileName name,
        MimeType mimeType,
        FileSize size,
        StorageObjectKey stagingObjectKey,
        StorageProvider provider,
        StorageFileType fileType,
        DateTime now,
        long creatorId = 0)
    {
        Guard.Positive(applicationId, nameof(applicationId));

        var file = new StorageFile
        {
            ApplicationId = applicationId,
            ParentFolderId = parentFolderId,
            Name = name,
            MimeType = mimeType,
            Size = size,
            ContentHash = ContentHash.FromString(PendingContentHashValue),
            ObjectKey = stagingObjectKey,
            Provider = provider,
            FileType = fileType,
            UploadStatus = UploadStatus.Pending,
            ThumbnailStatus = ThumbnailStatus.None,
            ConversionStatus = ConversionStatus.None
        };

        file.SetCreated(now);
        file.SetCreator(creatorId);
        return file;
    }

    public const string PendingContentHashValue = "pending";

    /// <summary>
    /// Rehydrates an aggregate from persisted state without raising domain events.
    /// Used exclusively by Infrastructure repositories (see AssemblyInfo.cs InternalsVisibleTo).
    /// </summary>
    internal static StorageFile FromPersistence(
        long id,
        Guid businessId,
        long applicationId,
        long? parentFolderId,
        FileName name,
        MimeType mimeType,
        FileSize size,
        ContentHash contentHash,
        StorageObjectKey objectKey,
        StorageObjectKey? thumbnailObjectKey,
        StorageProvider provider,
        StorageFileType fileType,
        ThumbnailStatus thumbnailStatus,
        ConversionStatus conversionStatus,
        UploadStatus uploadStatus,
        OcrStatus ocrStatus,
        FileMetadata metadata,
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
            MimeType = mimeType,
            Size = size,
            ContentHash = contentHash,
            ObjectKey = objectKey,
            ThumbnailObjectKey = thumbnailObjectKey,
            Provider = provider,
            FileType = fileType,
            ThumbnailStatus = thumbnailStatus,
            ConversionStatus = conversionStatus,
            UploadStatus = uploadStatus,
            OcrStatus = ocrStatus,
            Metadata = metadata,
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
        Apply(new StorageFileMovedToTrashDomainEvent(BusinessId, ApplicationId, Id, Name.Value, ParentFolderId, now));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        DeletionTime = null;
        Apply(new StorageFileRestoredDomainEvent(BusinessId, ApplicationId, Id, Name.Value, ParentFolderId));
    }

    public void AttachThumbnail(StorageObjectKey thumbnailObjectKey)
    {
        ThumbnailObjectKey = thumbnailObjectKey;
        ThumbnailStatus = ThumbnailStatus.Completed;
        Apply(new StorageFileThumbnailGeneratedDomainEvent(BusinessId, ApplicationId, Id, thumbnailObjectKey.Value));
    }

    public void FailThumbnail() => ThumbnailStatus = ThumbnailStatus.Failed;

    public void StartConversion() => ConversionStatus = ConversionStatus.Processing;
    public void CompleteConversion() => ConversionStatus = ConversionStatus.Completed;
    public void FailConversion() => ConversionStatus = ConversionStatus.Failed;

    public void StartUploadProcessing(DateTime now)
    {
        UploadStatus = UploadStatus.Processing;
        SetLastModification(now);
    }

    public void CompleteUpload(
        FileName name,
        MimeType mimeType,
        FileSize size,
        ContentHash contentHash,
        StorageObjectKey objectKey,
        StorageFileType fileType,
        bool converted,
        DateTime now)
    {
        Name = name;
        MimeType = mimeType;
        Size = size;
        ContentHash = contentHash;
        ObjectKey = objectKey;
        FileType = fileType;
        UploadStatus = UploadStatus.Completed;
        ThumbnailStatus = fileType is StorageFileType.Image or StorageFileType.Video
            ? ThumbnailStatus.Pending
            : ThumbnailStatus.None;

        if (converted)
            ConversionStatus = ConversionStatus.Completed;

        SetLastModification(now);
        Apply(new StorageFileUploadedDomainEvent(
            BusinessId, ApplicationId, name.Value, mimeType.Value, size.Bytes, fileType, ParentFolderId));
    }

    public void FailUpload(DateTime now)
    {
        UploadStatus = UploadStatus.Failed;
        SetLastModification(now);
    }

    public void StartOcr() => OcrStatus = OcrStatus.Processing;
    public void CompleteOcr() => OcrStatus = OcrStatus.Completed;
    public void FailOcr() => OcrStatus = OcrStatus.Failed;

    public void UpdateMetadata(FileMetadata metadata, DateTime now, long modifierId = 0)
    {
        Metadata = metadata;
        SetLastModification(now);
        SetLastModifier(modifierId);
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

    private void Apply(StorageFileUploadedDomainEvent @event) => AddEvent(@event);
    private void Apply(StorageFileMovedToTrashDomainEvent @event) => AddEvent(@event);
    private void Apply(StorageFileRestoredDomainEvent @event) => AddEvent(@event);
    private void Apply(StorageFileThumbnailGeneratedDomainEvent @event) => AddEvent(@event);
}
