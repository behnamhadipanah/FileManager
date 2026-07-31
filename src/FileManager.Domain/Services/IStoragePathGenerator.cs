using FileManager.Domain.Enumerations;
using FileManager.Domain.ValueObjects;

namespace FileManager.Domain.Services;

public interface IStoragePathGenerator
{
    StorageObjectKey GenerateObjectKey(Guid fileBusinessId);
    StorageObjectKey GenerateThumbnailObjectKey(Guid fileBusinessId);
}
