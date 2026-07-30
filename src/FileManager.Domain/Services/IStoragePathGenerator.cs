using FileManager.Domain.Enumerations;
using FileManager.Domain.ValueObjects;

namespace FileManager.Domain.Services;

public interface IStoragePathGenerator
{
    StorageObjectKey GenerateObjectKey(long applicationId, StorageFileType fileType, string uniqueFileName);
    StorageObjectKey GenerateThumbnailObjectKey(StorageObjectKey originalObjectKey);
}
