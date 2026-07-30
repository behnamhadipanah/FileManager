using FileManager.Domain.Common;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public sealed class StorageObjectKey : BaseValueObject<StorageObjectKey>
{
    public string Value { get; private set; } = null!;

    private StorageObjectKey() { }

    private StorageObjectKey(string value)
    {
        Guard.NotEmpty(value, nameof(value));
        Guard.MaxLength(value, 500);
        Value = value;
    }

    public static StorageObjectKey FromString(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static explicit operator string(StorageObjectKey key) => key.Value;
}
