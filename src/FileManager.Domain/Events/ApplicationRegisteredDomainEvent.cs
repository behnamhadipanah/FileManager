using Kootam.Framework.Domain.Events;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Domain.Events;

public sealed record ApplicationRegisteredDomainEvent(
    BusinessId BusinessId,
    string ApplicationName) : IDomainEvent;
