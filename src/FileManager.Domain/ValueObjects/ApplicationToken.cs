using FileManager.Domain.Common;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public sealed class ApplicationToken : BaseValueObject<ApplicationToken>
{
    public string Value { get; private set; } = null!;

    private ApplicationToken() { }

    private ApplicationToken(string value)
    {
        Guard.NotEmpty(value, nameof(value));
        Guard.MinLength(value, 32);
        Guard.MaxLength(value, 256);
        Value = value;
    }

    public static ApplicationToken FromString(string value) => new(value);

    public static ApplicationToken Generate()
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return new ApplicationToken(token[..32]);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
