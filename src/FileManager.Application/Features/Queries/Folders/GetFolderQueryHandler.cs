using FileManager.Application.Features.Queries.Folders;
using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Queries.Folders;

public sealed class GetFolderQueryHandler(IFolderRepository folderRepository)
    : IQueryHandler<GetFolderQuery, FolderResponse>
{
    public async Task<Result<FolderResponse>> Handle(
        GetFolderQuery query, CancellationToken cancellationToken = default)
    {
        var folder = await folderRepository.GetByBusinessIdAsync(
            query.ApplicationId, BusinessId.FromGuid(query.FolderBusinessId), cancellationToken);

        if (folder is null)
            return Result<FolderResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (query.IsDeleted is not null && folder.IsDeleted != query.IsDeleted)
            return Result<FolderResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        return Result<FolderResponse>.Success(
            await FolderMapper.ToResponseAsync(folder, folderRepository, cancellationToken));
    }
}
