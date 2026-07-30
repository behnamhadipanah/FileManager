using FileManager.Domain.Common;
using FileManager.Domain.Events;
using FileManager.Domain.ValueObjects;
using Kootam.Framework.Domain.Contracts.Capability;
using Kootam.Framework.Domain.Entities;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Aggregates.ApplicationAgg;

public sealed class RegisteredApplication : AggregateRoot, ICreationAuditedObject<long>, IActiveObject
{
    public string ApplicationName { get; private set; } = null!;
    public ApplicationToken Token { get; private set; } = null!;
    public UploadLimits UploadLimits { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreationTime { get; private set; }
    public long CreatorId { get; private set; }

    private RegisteredApplication() { }

    public static RegisteredApplication Register(
        string applicationName,
        ApplicationToken token,
        UploadLimits uploadLimits,
        DateTime now,
        long creatorId = 0)
    {
        Guard.NotEmpty(applicationName, nameof(applicationName));

        var application = new RegisteredApplication
        {
            ApplicationName = applicationName.Trim(),
            Token = token,
            UploadLimits = uploadLimits,
            IsActive = true
        };

        application.SetCreated(now);
        application.SetCreator(creatorId);
        application.Apply(new ApplicationRegisteredDomainEvent(application.BusinessId, application.ApplicationName));

        return application;
    }

    /// <summary>
    /// Rehydrates an aggregate from persisted state without raising domain events.
    /// Used exclusively by Infrastructure repositories (see AssemblyInfo.cs InternalsVisibleTo).
    /// </summary>
    internal static RegisteredApplication FromPersistence(
        long id,
        Guid businessId,
        string applicationName,
        ApplicationToken token,
        UploadLimits uploadLimits,
        bool isActive,
        DateTime creationTime,
        long creatorId,
        DateTime? lastModificationTime,
        long? lastModifierId) =>
        new()
        {
            Id = id,
            BusinessId = BusinessId.FromGuid(businessId),
            ApplicationName = applicationName,
            Token = token,
            UploadLimits = uploadLimits,
            IsActive = isActive,
            CreationTime = creationTime,
            CreatorId = creatorId,
            LastModificationTime = lastModificationTime,
            LastModifierId = lastModifierId
        };

    public void Deactivate(DateTime now, long modifierId = 0)
    {
        IsActive = false;
        SetLastModification(now);
        SetLastModifier(modifierId);
    }

    public void Activate(DateTime now, long modifierId = 0)
    {
        IsActive = true;
        SetLastModification(now);
        SetLastModifier(modifierId);
    }

    public void UpdateUploadLimits(UploadLimits uploadLimits, DateTime now, long modifierId = 0)
    {
        UploadLimits = uploadLimits;
        SetLastModification(now);
        SetLastModifier(modifierId);
    }

    public void RegenerateToken(ApplicationToken newToken, DateTime now, long modifierId = 0)
    {
        Token = newToken;
        SetLastModification(now);
        SetLastModifier(modifierId);
    }

    public bool ValidateToken(string token) => Token.Value == token;

    public DateTime? LastModificationTime { get; private set; }
    public long? LastModifierId { get; private set; }

    public void SetCreated(DateTime now) => CreationTime = now;
    public void SetCreator(long creatorId) => CreatorId = creatorId;
    public void SetLastModification(DateTime dateTime) => LastModificationTime = dateTime;
    public void SetLastModifier(long userId) => LastModifierId = userId;

    /// <summary>
    /// Assigns the database-generated identity after insert. No ORM is used, so
    /// repositories in the Infrastructure layer must push the generated Id back
    /// onto the aggregate themselves (see AssemblyInfo.cs InternalsVisibleTo).
    /// </summary>
    internal void AssignId(long id) => Id = id;

    private void Apply(ApplicationRegisteredDomainEvent @event) => AddEvent(@event);
}
