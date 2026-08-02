using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Applications;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;

namespace FileManager.Application.Features.Queries.Applications;

public sealed class GetApplicationQueryHandler(IApplicationRepository applicationRepository)
    : IQueryHandler<GetApplicationQuery, ApplicationResponse>
{
    public async Task<Result<ApplicationResponse>> Handle(
        GetApplicationQuery query, CancellationToken cancellationToken = default)
    {
        var application = await applicationRepository.GetAsync(query.ApplicationId, cancellationToken);
        if (application is null)
            return Result<ApplicationResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        return Result<ApplicationResponse>.Success(ApplicationMapper.ToResponse(application));
    }
}
