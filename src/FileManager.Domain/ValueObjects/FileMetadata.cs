using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.ValueObjects;

public sealed class FileMetadata : BaseValueObject<FileMetadata>
{
    public IReadOnlyDictionary<string, string> Values { get; private set; } =
        new Dictionary<string, string>();

    private FileMetadata() { }

    private FileMetadata(IReadOnlyDictionary<string, string> values)
    {
        Values = values;
    }

    public static FileMetadata Empty() => new(new Dictionary<string, string>());

    public static FileMetadata FromDictionary(IReadOnlyDictionary<string, string>? values) =>
        values is null || values.Count == 0
            ? Empty()
            : new FileMetadata(new Dictionary<string, string>(values));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var pair in Values.OrderBy(x => x.Key))
        {
            yield return pair.Key;
            yield return pair.Value;
        }
    }
}
