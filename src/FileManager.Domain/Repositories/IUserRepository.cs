using FileManager.Domain.Aggregates.UserAgg;

namespace FileManager.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task InsertAsync(User user, CancellationToken cancellationToken);
}
