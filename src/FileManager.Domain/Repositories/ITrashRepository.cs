using FileManager.Domain.Aggregates.TrashAgg;
using FileManager.Domain.Enumerations;

namespace FileManager.Domain.Repositories;

public interface ITrashRepository
{
    Task<TrashItem?> GetAsync(long applicationId, long id, CancellationToken cancellationToken);
    Task<TrashItem?> GetByItemAsync(long applicationId, TrashItemType itemType, long itemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TrashItem>> GetActiveByApplicationIdAsync(long applicationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TrashItem>> GetExpiredAsync(DateTime olderThan, CancellationToken cancellationToken);
    Task InsertAsync(TrashItem trashItem, CancellationToken cancellationToken);
    Task UpdateAsync(TrashItem trashItem, CancellationToken cancellationToken);
    Task DeleteAsync(long applicationId, long id, CancellationToken cancellationToken);
}
