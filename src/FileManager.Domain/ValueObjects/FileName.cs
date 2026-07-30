using FileManager.Domain.Common;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public class FileName:BaseValueObject<FileName>
{
    public string  Value { get; private set; }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    private FileName() { }

    private FileName(string value)
    {
        Guard.NotEmpty(value, nameof(value));
        Guard.MinLength(value, 1);
        Guard.MaxLength(value, 255);
        Value = value;
    }

    public static FileName FromString(string value) => new FileName(value);
    
    public static implicit operator FileName(string? value) => FromString(value);
    public static explicit operator string(FileName value) => value.Value;
}