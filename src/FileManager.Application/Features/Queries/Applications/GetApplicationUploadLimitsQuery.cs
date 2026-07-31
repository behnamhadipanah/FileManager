using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Applications;
using Kootam.Cqrs.Abstractions.Queries;

namespace FileManager.Application.Features.Queries.Applications;

public sealed record GetApplicationUploadLimitsQuery(long ApplicationId)
    : IQuery<ApplicationUploadLimitsResponse>;
