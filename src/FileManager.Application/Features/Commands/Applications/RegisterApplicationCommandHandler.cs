using FileManager.Contracts.Responses.Applications;
using FileManager.Domain.Aggregates.ApplicationAgg;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using FileManager.Domain.ValueObjects;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Features.Commands.Applications;

public sealed class RegisterApplicationCommandHandler(IApplicationRepository applicationRepository)
    : IRequestHandler<RegisterApplicationCommand, RegisterApplicationResponse>
{
    public async Task<Result<RegisterApplicationResponse>> Handle(
        RegisterApplicationCommand command, CancellationToken cancellationToken = default)
    {
        if (await applicationRepository.ExistsByNameAsync(command.ApplicationName, cancellationToken))
            return Result<RegisterApplicationResponse>.Failure(ResultStatus.Conflict, DomainMessages.ApplicationNameExists);

        var uploadLimits = UploadLimits.Create(
            command.MinSizeUploadImage,
            command.MaxSizeUploadImage,
            command.MinSizeVideo,
            command.MaxSizeVideo,
            command.MinSizeDcoument,
            command.MaxSizeDcoument);

        var application = RegisteredApplication.Register(
            command.ApplicationName,
            ApplicationToken.Generate(),
            uploadLimits,
            DateTime.UtcNow);

        await applicationRepository.InsertAsync(application, cancellationToken);

        return Result<RegisterApplicationResponse>.Success(new RegisterApplicationResponse
        {
            ApplicationId = application.Id,
            Token = application.Token.Value
        });
    }
}
