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

    /// <summary>Exact kilobytes when size was created from whole KB values.</summary>
    public long Kilobytes => Bytes / 1024;

    /// <summary>Rounds up to the next kilobyte for uploaded file sizes.</summary>
    public long ToKilobytes() => Bytes == 0 ? 0 : (Bytes + 1023) / 1024;

    public bool IsWithinRange(FileSize min, FileSize max) =>
        Bytes >= min.Bytes && Bytes <= max.Bytes;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Bytes;
    }
}
