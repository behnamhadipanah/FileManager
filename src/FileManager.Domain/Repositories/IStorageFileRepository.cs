using FileManager.Domain.Aggregates.FileAgg;
using FileManager.Domain.ValueObjects;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Repositories;

public interface IStorageFileRepository
{
    Task<StorageFile?> GetAsync(long applicationId, long id, CancellationToken cancellationToken);
    Task<StorageFile?> GetByBusinessIdAsync(long applicationId, BusinessId businessId, CancellationToken cancellationToken);
    Task<StorageFile?> GetByContentHashAsync(long applicationId, ContentHash contentHash, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(long applicationId, long id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(long applicationId, long? parentFolderId, string name, CancellationToken cancellationToken);
    Task<IReadOnlyList<StorageFile>> GetByParentFolderIdAsync(
        long applicationId,
        long parentFolderId,
        bool? isDeleted,
        CancellationToken cancellationToken);
    Task InsertAsync(StorageFile file, CancellationToken cancellationToken);
    Task UpdateAsync(StorageFile file, CancellationToken cancellationToken);
    Task DeleteAsync(long applicationId, long id, CancellationToken cancellationToken);
}
