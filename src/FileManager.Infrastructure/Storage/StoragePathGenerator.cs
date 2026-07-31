using FileManager.Domain.Services;
using FileManager.Domain.ValueObjects;

namespace FileManager.Infrastructure.Storage;

public sealed class StoragePathGenerator : IStoragePathGenerator
{
    public StorageObjectKey GenerateObjectKey(Guid fileBusinessId) =>
        StorageObjectKey.FromString(fileBusinessId.ToString("D"));

    public StorageObjectKey GenerateThumbnailObjectKey(Guid fileBusinessId) =>
        StorageObjectKey.FromString(fileBusinessId.ToString("D"));
}
