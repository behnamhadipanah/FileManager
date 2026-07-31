using FileManager.Domain.Aggregates.UserAgg;
using FileManager.Domain.Repositories;
using Kootam.Authentication.Abstractions.Services;

namespace FileManager.Infrastructure.Seeding;

public sealed class UserSeeder(IUserRepository userRepository, IPasswordHasherService passwordHasher)
{
    private const string AdminEmail = "admin@admin.com";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await userRepository.ExistsByEmailAsync(AdminEmail, cancellationToken))
            return;

        var user = User.Create(
            firstName: "Behnam",
            lastName: "Hadipanah",
            email: AdminEmail,
            passwordHash: passwordHasher.HashPassword("admin"),
            now: DateTime.UtcNow);

        await userRepository.InsertAsync(user, cancellationToken);
    }
}
