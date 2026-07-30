using FileManager.Domain.Enumerations;
using FileManager.Domain.Services;
using FileManager.Domain.ValueObjects;

namespace FileManager.Infrastructure.Storage;

public sealed class StoragePathGenerator : IStoragePathGenerator
{
    public StorageObjectKey GenerateObjectKey(long applicationId, StorageFileType fileType, string uniqueFileName)
    {
        var folder = fileType.ToString().ToLowerInvariant();
        var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd");
        return StorageObjectKey.FromString($"{applicationId}/{folder}/{datePath}/{uniqueFileName}");
    }

    public StorageObjectKey GenerateThumbnailObjectKey(StorageObjectKey originalObjectKey)
    {
        var original = (string)originalObjectKey;
        var withoutExtension = Path.ChangeExtension(original, null);
        return StorageObjectKey.FromString($"{withoutExtension}_thumb.jpg");
    }
}
