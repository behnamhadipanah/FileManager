namespace FileManager.Domain.Aggregates.UserAgg;

public sealed class User
{
    public long Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreationTime { get; private set; }

    private User() { }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        DateTime now) =>
        new()
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            IsActive = true,
            CreationTime = now
        };

    internal static User FromPersistence(
        long id,
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        bool isActive,
        DateTime creationTime) =>
        new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = isActive,
            CreationTime = creationTime
        };

    internal void AssignId(long id) => Id = id;
}
