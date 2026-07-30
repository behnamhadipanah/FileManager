using FileManager.Domain.Aggregates.ApplicationAgg;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Repositories;

public interface IApplicationRepository
{
    Task<RegisteredApplication?> GetAsync(long id, CancellationToken cancellationToken);
    Task<RegisteredApplication?> GetByBusinessIdAsync(BusinessId businessId, CancellationToken cancellationToken);
    Task<RegisteredApplication?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string applicationName, CancellationToken cancellationToken);
    Task InsertAsync(RegisteredApplication application, CancellationToken cancellationToken);
    Task UpdateAsync(RegisteredApplication application, CancellationToken cancellationToken);
}
