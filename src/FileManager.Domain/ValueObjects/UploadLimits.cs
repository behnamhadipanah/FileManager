using FileManager.Domain.Common;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public sealed class UploadLimits : BaseValueObject<UploadLimits>
{
    public long MinImageSizeKilobytes { get; private set; }
    public long MaxImageSizeKilobytes { get; private set; }
    public long MinVideoSizeKilobytes { get; private set; }
    public long MaxVideoSizeKilobytes { get; private set; }
    public long MinDocumentSizeKilobytes { get; private set; }
    public long MaxDocumentSizeKilobytes { get; private set; }

    public FileSize MinImageSize => FileSize.FromKilobytes(MinImageSizeKilobytes);
    public FileSize MaxImageSize => FileSize.FromKilobytes(MaxImageSizeKilobytes);
    public FileSize MinVideoSize => FileSize.FromKilobytes(MinVideoSizeKilobytes);
    public FileSize MaxVideoSize => FileSize.FromKilobytes(MaxVideoSizeKilobytes);
    public FileSize MinDocumentSize => FileSize.FromKilobytes(MinDocumentSizeKilobytes);
    public FileSize MaxDocumentSize => FileSize.FromKilobytes(MaxDocumentSizeKilobytes);

    private UploadLimits() { }

    private UploadLimits(
        long minImageSizeKilobytes,
        long maxImageSizeKilobytes,
        long minVideoSizeKilobytes,
        long maxVideoSizeKilobytes,
        long minDocumentSizeKilobytes,
        long maxDocumentSizeKilobytes)
    {
        EnsureRange(minImageSizeKilobytes, maxImageSizeKilobytes, nameof(minImageSizeKilobytes));
        EnsureRange(minVideoSizeKilobytes, maxVideoSizeKilobytes, nameof(minVideoSizeKilobytes));
        EnsureRange(minDocumentSizeKilobytes, maxDocumentSizeKilobytes, nameof(minDocumentSizeKilobytes));

        MinImageSizeKilobytes = minImageSizeKilobytes;
        MaxImageSizeKilobytes = maxImageSizeKilobytes;
        MinVideoSizeKilobytes = minVideoSizeKilobytes;
        MaxVideoSizeKilobytes = maxVideoSizeKilobytes;
        MinDocumentSizeKilobytes = minDocumentSizeKilobytes;
        MaxDocumentSizeKilobytes = maxDocumentSizeKilobytes;
    }

    public static UploadLimits Create(
        long minImageSizeKilobytes,
        long maxImageSizeKilobytes,
        long minVideoSizeKilobytes,
        long maxVideoSizeKilobytes,
        long minDocumentSizeKilobytes,
        long maxDocumentSizeKilobytes) =>
        new(
            minImageSizeKilobytes,
            maxImageSizeKilobytes,
            minVideoSizeKilobytes,
            maxVideoSizeKilobytes,
            minDocumentSizeKilobytes,
            maxDocumentSizeKilobytes);

    public static UploadLimits FromPersistence(
        long minImageSizeKilobytes,
        long maxImageSizeKilobytes,
        long minVideoSizeKilobytes,
        long maxVideoSizeKilobytes,
        long minDocumentSizeKilobytes,
        long maxDocumentSizeKilobytes) =>
        new(
            minImageSizeKilobytes,
            maxImageSizeKilobytes,
            minVideoSizeKilobytes,
            maxVideoSizeKilobytes,
            minDocumentSizeKilobytes,
            maxDocumentSizeKilobytes);

    public (FileSize Min, FileSize Max) GetLimitsFor(StorageFileType fileType) =>
        fileType switch
        {
            StorageFileType.Image => (MinImageSize, MaxImageSize),
            StorageFileType.Video => (MinVideoSize, MaxVideoSize),
            StorageFileType.Document => (MinDocumentSize, MaxDocumentSize),
            _ => (FileSize.Zero, FileSize.FromBytes(long.MaxValue))
        };

    public (long MinKilobytes, long MaxKilobytes) GetKilobyteLimitsFor(StorageFileType fileType) =>
        fileType switch
        {
            StorageFileType.Image => (MinImageSizeKilobytes, MaxImageSizeKilobytes),
            StorageFileType.Video => (MinVideoSizeKilobytes, MaxVideoSizeKilobytes),
            StorageFileType.Document => (MinDocumentSizeKilobytes, MaxDocumentSizeKilobytes),
            _ => (0, long.MaxValue)
        };

    private static void EnsureRange(long minKilobytes, long maxKilobytes, string field)
    {
        if (minKilobytes > maxKilobytes)
            throw new Exceptions.DomainException(DomainMessages.InvalidUploadLimitRange, field);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MinImageSizeKilobytes;
        yield return MaxImageSizeKilobytes;
        yield return MinVideoSizeKilobytes;
        yield return MaxVideoSizeKilobytes;
        yield return MinDocumentSizeKilobytes;
        yield return MaxDocumentSizeKilobytes;
    }
}
