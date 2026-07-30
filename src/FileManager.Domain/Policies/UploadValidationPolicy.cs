using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.ValueObjects;

namespace FileManager.Domain.Policies;

public static class UploadValidationPolicy
{
    public static StorageFileType ResolveFileType(MimeType mimeType) =>
        mimeType switch
        {
            _ when mimeType.IsImage() => StorageFileType.Image,
            _ when mimeType.IsVideo() => StorageFileType.Video,
            _ when mimeType.IsDocument() => StorageFileType.Document,
            _ => StorageFileType.Other
        };

    public static void EnsureWithinLimits(UploadLimits limits, StorageFileType fileType, FileSize size)
    {
        var (min, max) = limits.GetLimitsFor(fileType);
        if (!size.IsWithinRange(min, max))
            throw new Exceptions.DomainException(DomainMessages.FileSizeOutOfRange, fileType.ToString());
    }
}
