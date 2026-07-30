using FileManager.Domain.Enumerations;
using Kootam.Framework.Domain.Events;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Events;

public sealed record StorageFileUploadedDomainEvent(
    BusinessId BusinessId,
    long ApplicationId,
    string Name,
    string MimeType,
    long SizeBytes,
    StorageFileType FileType,
    long? ParentFolderId) : IDomainEvent;
