using FileManager.Contracts.Responses.Applications;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using FileManager.Domain.ValueObjects;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Features.Commands.Applications;

public sealed class RegenerateApplicationTokenCommandHandler(IApplicationRepository applicationRepository)
    : IRequestHandler<RegenerateApplicationTokenCommand, RegenerateApplicationTokenResponse>
{
    public async Task<Result<RegenerateApplicationTokenResponse>> Handle(
        RegenerateApplicationTokenCommand command, CancellationToken cancellationToken = default)
    {
        var application = await applicationRepository.GetAsync(command.ApplicationId, cancellationToken);
        if (application is null)
            return Result<RegenerateApplicationTokenResponse>.Failure(
                ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        if (!application.IsActive)
            return Result<RegenerateApplicationTokenResponse>.Failure(
                ResultStatus.ValidationError, DomainMessages.ApplicationInactive);

        application.RegenerateToken(ApplicationToken.Generate(), DateTime.UtcNow);
        await applicationRepository.UpdateAsync(application, cancellationToken);

        return Result<RegenerateApplicationTokenResponse>.Success(new RegenerateApplicationTokenResponse
        {
            ApplicationId = application.Id,
            Token = application.Token.Value
        });
    }
}
