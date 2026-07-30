using FileManager.Domain.Common;
using FileManager.Domain.Messages;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public sealed class UploadLimits : BaseValueObject<UploadLimits>
{
    public FileSize MinImageSize { get; private set; } = FileSize.Zero;
    public FileSize MaxImageSize { get; private set; } = FileSize.Zero;
    public FileSize MinVideoSize { get; private set; } = FileSize.Zero;
    public FileSize MaxVideoSize { get; private set; } = FileSize.Zero;
    public FileSize MinDocumentSize { get; private set; } = FileSize.Zero;
    public FileSize MaxDocumentSize { get; private set; } = FileSize.Zero;

    private UploadLimits() { }

    private UploadLimits(
        FileSize minImageSize,
        FileSize maxImageSize,
        FileSize minVideoSize,
        FileSize maxVideoSize,
        FileSize minDocumentSize,
        FileSize maxDocumentSize)
    {
        EnsureRange(minImageSize, maxImageSize, nameof(minImageSize));
        EnsureRange(minVideoSize, maxVideoSize, nameof(minVideoSize));
        EnsureRange(minDocumentSize, maxDocumentSize, nameof(minDocumentSize));

        MinImageSize = minImageSize;
        MaxImageSize = maxImageSize;
        MinVideoSize = minVideoSize;
        MaxVideoSize = maxVideoSize;
        MinDocumentSize = minDocumentSize;
        MaxDocumentSize = maxDocumentSize;
    }

    public static UploadLimits Create(
        long minImageBytes,
        long maxImageBytes,
        long minVideoBytes,
        long maxVideoBytes,
        long minDocumentBytes,
        long maxDocumentBytes) =>
        new(
            FileSize.FromBytes(minImageBytes),
            FileSize.FromBytes(maxImageBytes),
            FileSize.FromBytes(minVideoBytes),
            FileSize.FromBytes(maxVideoBytes),
            FileSize.FromBytes(minDocumentBytes),
            FileSize.FromBytes(maxDocumentBytes));

    public (FileSize Min, FileSize Max) GetLimitsFor(Enumerations.StorageFileType fileType) =>
        fileType switch
        {
            Enumerations.StorageFileType.Image => (MinImageSize, MaxImageSize),
            Enumerations.StorageFileType.Video => (MinVideoSize, MaxVideoSize),
            Enumerations.StorageFileType.Document => (MinDocumentSize, MaxDocumentSize),
            _ => (FileSize.Zero, FileSize.FromBytes(long.MaxValue))
        };

    private static void EnsureRange(FileSize min, FileSize max, string field)
    {
        if (min.Bytes > max.Bytes)
            throw new Exceptions.DomainException(DomainMessages.InvalidUploadLimitRange, field);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MinImageSize;
        yield return MaxImageSize;
        yield return MinVideoSize;
        yield return MaxVideoSize;
        yield return MinDocumentSize;
        yield return MaxDocumentSize;
    }
}
