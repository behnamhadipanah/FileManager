using FileManager.Domain.Aggregates.FolderAgg;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Repositories;

public interface IFolderRepository
{
    Task<Folder?> GetAsync(long applicationId, long id, CancellationToken cancellationToken);
    Task<Folder?> GetByBusinessIdAsync(long applicationId, BusinessId businessId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(long applicationId, long id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(long applicationId, long? parentFolderId, string name, CancellationToken cancellationToken);
    Task<bool> HasChildrenAsync(long applicationId, long folderId, CancellationToken cancellationToken);
    Task InsertAsync(Folder folder, CancellationToken cancellationToken);
    Task UpdateAsync(Folder folder, CancellationToken cancellationToken);
}
