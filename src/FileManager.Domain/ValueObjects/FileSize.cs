using FileManager.Domain.Common;
using FileManager.Domain.Messages;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public sealed class FileSize : BaseValueObject<FileSize>
{
    public long Bytes { get; private set; }

    private FileSize() { }

    private FileSize(long bytes)
    {
        Guard.NotNegative(bytes, nameof(bytes));
        Bytes = bytes;
    }

    public static FileSize FromBytes(long bytes) => new(bytes);

    public static FileSize FromKilobytes(long kilobytes) => new(kilobytes * 1024);

    public static FileSize Zero => new(0);

    public bool IsWithinRange(FileSize min, FileSize max) =>
        Bytes >= min.Bytes && Bytes <= max.Bytes;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Bytes;
    }
}
