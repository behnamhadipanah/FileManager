using FileManager.Application.Features.Queries.Applications;
using FileManager.Contracts.Responses.Applications;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;

namespace FileManager.Application.Features.Queries.Applications;

public sealed class GetApplicationsQueryHandler(IApplicationRepository applicationRepository)
    : IQueryHandler<GetApplicationsQuery, IReadOnlyList<ApplicationMenuItemResponse>>
{
    public async Task<Result<IReadOnlyList<ApplicationMenuItemResponse>>> Handle(
        GetApplicationsQuery query, CancellationToken cancellationToken = default)
    {
        var applications = await applicationRepository.GetAllActiveAsync(cancellationToken);

        var response = applications
            .Select(app => new ApplicationMenuItemResponse
            {
                Id = app.Id,
                BusinessId = (Guid)app.BusinessId,
                ApplicationName = app.ApplicationName
            })
            .ToList();

        return Result<IReadOnlyList<ApplicationMenuItemResponse>>.Success(response);
    }
}
