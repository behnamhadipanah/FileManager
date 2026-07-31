using FileManager.Application.Features.Commands.Folders;
using FileManager.Application.Features.Commands.Trash;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using FileManager.Contracts.Responses.Trash;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Features.Commands.Trash;

public sealed class RestoreTrashItemCommandHandler(
    IFolderRepository folderRepository,
    IStorageFileRepository storageFileRepository,
    ITrashRepository trashRepository)
    : IRequestHandler<RestoreTrashItemCommand, RestoreTrashItemResponse>
{
    public async Task<Result<RestoreTrashItemResponse>> Handle(
        RestoreTrashItemCommand command, CancellationToken cancellationToken = default)
    {
        var trashItem = await trashRepository.GetAsync(command.ApplicationId, command.TrashItemId, cancellationToken);
        if (trashItem is null)
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.NotFound, DomainMessages.TrashItemNotFound);

        if (trashItem.IsPurged)
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.Conflict, DomainMessages.TrashItemAlreadyPurged);

        if (trashItem.IsRestored)
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.Conflict, DomainMessages.TrashItemAlreadyRestored);

        var now = DateTime.UtcNow;
        var (restoredParentId, errorStatus, errorMessage) = await FolderHierarchyRestorer.EnsureParentFolderChainAsync(
            folderRepository, trashRepository, command.ApplicationId, trashItem.OriginalParentFolderId, now, cancellationToken);

        if (errorStatus is not null)
            return Result<RestoreTrashItemResponse>.Failure(errorStatus.Value, errorMessage!);

        return trashItem.ItemType switch
        {
            TrashItemType.File => await RestoreFileAsync(trashItem, restoredParentId, now, cancellationToken),
            TrashItemType.Folder => await RestoreFolderAsync(trashItem, restoredParentId, now, cancellationToken),
            _ => Result<RestoreTrashItemResponse>.Failure(ResultStatus.ValidationError, DomainMessages.UnsupportedFileType)
        };
    }

    private async Task<Result<RestoreTrashItemResponse>> RestoreFileAsync(
        Domain.Aggregates.TrashAgg.TrashItem trashItem, long? restoredParentId, DateTime now, CancellationToken cancellationToken)
    {
        var file = await storageFileRepository.GetAsync(trashItem.ApplicationId, trashItem.ItemId, cancellationToken);
        if (file is null)
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.NotFound, DomainMessages.FileNotFound);

        if (!file.IsDeleted)
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileNotInTrash);

        if (file.ParentFolderId != restoredParentId)
            file.Move(restoredParentId, now);

        if (await storageFileRepository.ExistsByNameAsync(
                trashItem.ApplicationId, restoredParentId, file.Name.Value, cancellationToken))
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileNameExists);

        file.Restore();
        trashItem.Restore(now);

        await storageFileRepository.UpdateAsync(file, cancellationToken);
        await trashRepository.UpdateAsync(trashItem, cancellationToken);

        return Result<RestoreTrashItemResponse>.Success(new RestoreTrashItemResponse
        {
            ItemType = Contracts.Enumerations.TrashItemType.File,
            ItemId = file.Id,
            ItemName = file.Name.Value,
            RestoredToFolderId = restoredParentId
        });
    }

    private async Task<Result<RestoreTrashItemResponse>> RestoreFolderAsync(
        Domain.Aggregates.TrashAgg.TrashItem trashItem, long? restoredParentId, DateTime now, CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetAsync(trashItem.ApplicationId, trashItem.ItemId, cancellationToken);
        if (folder is null)
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (!folder.IsDeleted)
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileNotInTrash);

        if (folder.ParentFolderId != restoredParentId)
            folder.Move(restoredParentId, now);

        if (await folderRepository.ExistsByNameAsync(
                trashItem.ApplicationId, restoredParentId, folder.Name.Value, cancellationToken))
            return Result<RestoreTrashItemResponse>.Failure(ResultStatus.Conflict, DomainMessages.FolderNameExists);

        folder.Restore();
        trashItem.Restore(now);

        await folderRepository.UpdateAsync(folder, cancellationToken);
        await trashRepository.UpdateAsync(trashItem, cancellationToken);

        return Result<RestoreTrashItemResponse>.Success(new RestoreTrashItemResponse
        {
            ItemType = Contracts.Enumerations.TrashItemType.Folder,
            ItemId = folder.Id,
            ItemName = folder.Name.Value,
            RestoredToFolderId = restoredParentId
        });
    }
}
