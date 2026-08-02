using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Applications;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;

namespace FileManager.Application.Features.Queries.Applications;

public sealed class GetApplicationsQueryHandler(IApplicationRepository applicationRepository)
    : IQueryHandler<GetApplicationsQuery, IReadOnlyList<ApplicationResponse>>
{
    public async Task<Result<IReadOnlyList<ApplicationResponse>>> Handle(
        GetApplicationsQuery query, CancellationToken cancellationToken = default)
    {
        var applications = await applicationRepository.GetAllActiveAsync(cancellationToken);

        var response = applications
            .Select(ApplicationMapper.ToResponse)
            .ToList();

        return Result<IReadOnlyList<ApplicationResponse>>.Success(response);
    }
}
