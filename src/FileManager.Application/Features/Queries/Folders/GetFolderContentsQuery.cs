using FileManager.Contracts.Responses.Folders;
using Kootam.Cqrs.Abstractions.Queries;

namespace FileManager.Application.Features.Queries.Folders;

public sealed record GetFolderContentsQuery(
    long ApplicationId,
    Guid? FolderBusinessId,
    bool? IsDeleted) : IQuery<FolderContentsResponse>;
