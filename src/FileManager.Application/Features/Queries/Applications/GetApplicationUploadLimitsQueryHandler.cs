using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Applications;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;

namespace FileManager.Application.Features.Queries.Applications;

public sealed class GetApplicationUploadLimitsQueryHandler(IApplicationRepository applicationRepository)
    : IQueryHandler<GetApplicationUploadLimitsQuery, ApplicationUploadLimitsResponse>
{
    public async Task<Result<ApplicationUploadLimitsResponse>> Handle(
        GetApplicationUploadLimitsQuery query, CancellationToken cancellationToken = default)
    {
        var application = await applicationRepository.GetAsync(query.ApplicationId, cancellationToken);
        if (application is null)
            return Result<ApplicationUploadLimitsResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        return Result<ApplicationUploadLimitsResponse>.Success(
            ApplicationMapper.ToUploadLimitsResponse(application));
    }
}
