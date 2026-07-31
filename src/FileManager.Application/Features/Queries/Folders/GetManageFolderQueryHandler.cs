using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Queries.Folders;

public sealed class GetManageFolderQueryHandler(
    IApplicationRepository applicationRepository,
    IFolderRepository folderRepository)
    : IQueryHandler<GetManageFolderQuery, FolderResponse>
{
    public async Task<Result<FolderResponse>> Handle(
        GetManageFolderQuery query, CancellationToken cancellationToken = default)
    {
        var application = await applicationRepository.GetByBusinessIdAsync(
            BusinessId.FromGuid(query.ApplicationBusinessId), cancellationToken);

        if (application is null)
            return Result<FolderResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        var folder = query.FolderBusinessId is null
            ? await folderRepository.GetRootAsync(application.Id, cancellationToken)
            : await folderRepository.GetByBusinessIdAsync(
                application.Id,
                BusinessId.FromGuid(query.FolderBusinessId.Value),
                cancellationToken);

        if (folder is null)
            return Result<FolderResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (query.IsDeleted is not null && folder.IsDeleted != query.IsDeleted)
            return Result<FolderResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        return Result<FolderResponse>.Success(
            await FolderMapper.ToResponseAsync(folder, folderRepository, cancellationToken));
    }
}
