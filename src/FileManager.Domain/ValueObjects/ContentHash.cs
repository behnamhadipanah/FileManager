using FileManager.Domain.Common;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public sealed class ContentHash : BaseValueObject<ContentHash>
{
    public string Value { get; private set; } = null!;

    private ContentHash() { }

    private ContentHash(string value)
    {
        Guard.NotEmpty(value, nameof(value));
        Guard.MaxLength(value, 128);
        Value = value;
    }

    public static ContentHash FromString(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
