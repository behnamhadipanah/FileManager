using FileManager.Domain.Common;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public sealed class MimeType : BaseValueObject<MimeType>
{
    public string Value { get; private set; } = null!;

    private MimeType() { }

    private MimeType(string value)
    {
        Guard.NotEmpty(value, nameof(value));
        Guard.MaxLength(value, 200);
        Value = value.ToLowerInvariant();
    }

    public static MimeType FromString(string value) => new(value);

    public bool IsImage() => Value.StartsWith("image/", StringComparison.Ordinal);

    public bool IsVideo() => Value.StartsWith("video/", StringComparison.Ordinal);

    public bool IsDocument() =>
        Value.StartsWith("application/pdf", StringComparison.Ordinal)
        || Value.StartsWith("application/msword", StringComparison.Ordinal)
        || Value.StartsWith("application/vnd.", StringComparison.Ordinal)
        || Value.StartsWith("text/", StringComparison.Ordinal);

    public bool IsSvg() =>
        Value.Equals("image/svg+xml", StringComparison.Ordinal);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
