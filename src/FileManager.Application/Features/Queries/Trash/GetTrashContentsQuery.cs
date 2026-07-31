using FileManager.Contracts.Responses.Trash;
using Kootam.Cqrs.Abstractions.Queries;

namespace FileManager.Application.Features.Queries.Trash;

public sealed record GetTrashContentsQuery(long ApplicationId) : IQuery<TrashContentsResponse>;
