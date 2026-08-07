using FileManager.Application.Abstractions;
using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Trash;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;

namespace FileManager.Application.Features.Queries.Trash;

public sealed class GetTrashContentsQueryHandler(
    IApplicationRepository applicationRepository,
    ITrashRepository trashRepository,
    IFolderRepository folderRepository,
    IStorageFileRepository storageFileRepository,
    IFileStorageService fileStorageService)
    : IQueryHandler<GetTrashContentsQuery, TrashContentsResponse>
{
    public async Task<Result<TrashContentsResponse>> Handle(
        GetTrashContentsQuery query, CancellationToken cancellationToken = default)
    {
        if (!await applicationRepository.ExistsAsync(query.ApplicationId, cancellationToken))
            return Result<TrashContentsResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        var application = await applicationRepository.GetAsync(query.ApplicationId, cancellationToken);
        if (application is null)
            return Result<TrashContentsResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        var trashItems = await trashRepository.GetActiveByApplicationIdAsync(query.ApplicationId, cancellationToken);

        var folders = new List<TrashFolderItemResponse>();
        var files = new List<TrashFileItemResponse>();

        foreach (var trashItem in trashItems)
        {
            switch (trashItem.ItemType)
            {
                case TrashItemType.Folder:
                {
                    var folder = await folderRepository.GetAsync(
                        query.ApplicationId, trashItem.ItemId, cancellationToken);

                    if (folder is null)
                        break;

                    folders.Add(new TrashFolderItemResponse
                    {
                        TrashItemId = trashItem.Id,
                        Folder = await FolderMapper.ToResponseAsync(folder, folderRepository, cancellationToken),
                        OriginalParentFolderBusinessId = await ResolveParentBusinessIdAsync(
                            query.ApplicationId, trashItem.OriginalParentFolderId, cancellationToken),
                        TrashedTime = trashItem.CreationTime
                    });
                    break;
                }
                case TrashItemType.File:
                {
                    var file = await storageFileRepository.GetAsync(
                        query.ApplicationId, trashItem.ItemId, cancellationToken);

                    if (file is null)
                        break;

                    files.Add(new TrashFileItemResponse
                    {
                        TrashItemId = trashItem.Id,
                        File = await StorageFileMapper.ToResponseAsync(
                            file,
                            application.ApplicationName,
                            folderRepository,
                            fileStorageService,
                            cancellationToken),
                        OriginalParentFolderBusinessId = await ResolveParentBusinessIdAsync(
                            query.ApplicationId, trashItem.OriginalParentFolderId, cancellationToken),
                        TrashedTime = trashItem.CreationTime
                    });
                    break;
                }
            }
        }

        return Result<TrashContentsResponse>.Success(new TrashContentsResponse
        {
            Folders = folders,
            Files = files
        });
    }

    private async Task<Guid?> ResolveParentBusinessIdAsync(
        long applicationId, long? parentFolderId, CancellationToken cancellationToken)
    {
        if (parentFolderId is null)
            return null;

        var parent = await folderRepository.GetAsync(applicationId, parentFolderId.Value, cancellationToken);
        return parent is not null ? (Guid)parent.BusinessId : null;
    }
}
