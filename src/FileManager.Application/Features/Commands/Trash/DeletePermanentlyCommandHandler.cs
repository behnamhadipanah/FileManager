using FileManager.Application.Abstractions;
using FileManager.Application.Features.Commands.Trash;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using FileManager.Contracts.Responses.Trash;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Features.Commands.Trash;

public sealed class DeletePermanentlyCommandHandler(
    IApplicationRepository applicationRepository,
    IFolderRepository folderRepository,
    IStorageFileRepository storageFileRepository,
    ITrashRepository trashRepository,
    IFileStorageService fileStorageService)
    : IRequestHandler<DeletePermanentlyCommand, DeletePermanentlyResponse>
{
    public async Task<Result<DeletePermanentlyResponse>> Handle(
        DeletePermanentlyCommand command, CancellationToken cancellationToken = default)
    {
        var application = await applicationRepository.GetAsync(command.ApplicationId, cancellationToken);
        if (application is null)
            return Result<DeletePermanentlyResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        var trashItem = await trashRepository.GetAsync(command.ApplicationId, command.TrashItemId, cancellationToken);
        if (trashItem is null)
            return Result<DeletePermanentlyResponse>.Failure(ResultStatus.NotFound, DomainMessages.TrashItemNotFound);

        if (trashItem.IsPurged)
            return Result<DeletePermanentlyResponse>.Failure(ResultStatus.Conflict, DomainMessages.TrashItemAlreadyPurged);

        return trashItem.ItemType switch
        {
            TrashItemType.File => await DeleteFilePermanentlyAsync(application.ApplicationName, trashItem, cancellationToken),
            TrashItemType.Folder => await DeleteFolderPermanentlyAsync(trashItem, cancellationToken),
            _ => Result<DeletePermanentlyResponse>.Failure(ResultStatus.ValidationError, DomainMessages.UnsupportedFileType)
        };
    }

    private async Task<Result<DeletePermanentlyResponse>> DeleteFilePermanentlyAsync(
        string applicationName,
        Domain.Aggregates.TrashAgg.TrashItem trashItem,
        CancellationToken cancellationToken)
    {
        var file = await storageFileRepository.GetAsync(trashItem.ApplicationId, trashItem.ItemId, cancellationToken);
        if (file is null)
            return Result<DeletePermanentlyResponse>.Failure(ResultStatus.NotFound, DomainMessages.FileNotFound);

        if (!file.IsDeleted)
            return Result<DeletePermanentlyResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileNotInTrash);

        var storageContext = new ApplicationStorageContext(applicationName, file.FileType);

        await fileStorageService.DeleteFileAsync(storageContext, file.ObjectKey.Value, cancellationToken);

        if (file.ThumbnailObjectKey is not null)
            await fileStorageService.DeleteThumbnailAsync(storageContext, file.ThumbnailObjectKey.Value, cancellationToken);

        await storageFileRepository.DeleteAsync(trashItem.ApplicationId, file.Id, cancellationToken);
        await trashRepository.DeleteAsync(trashItem.ApplicationId, trashItem.Id, cancellationToken);

        return Result<DeletePermanentlyResponse>.Success(new DeletePermanentlyResponse
        {
            ItemType = Contracts.Enumerations.TrashItemType.File,
            ItemId = file.Id,
            ItemName = file.Name.Value
        });
    }

    private async Task<Result<DeletePermanentlyResponse>> DeleteFolderPermanentlyAsync(
        Domain.Aggregates.TrashAgg.TrashItem trashItem, CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetAsync(trashItem.ApplicationId, trashItem.ItemId, cancellationToken);
        if (folder is null)
            return Result<DeletePermanentlyResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (!folder.IsDeleted)
            return Result<DeletePermanentlyResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileNotInTrash);

        if (await folderRepository.HasChildrenAsync(trashItem.ApplicationId, folder.Id, cancellationToken))
            return Result<DeletePermanentlyResponse>.Failure(ResultStatus.ValidationError, DomainMessages.FolderNotEmpty);

        await folderRepository.DeleteAsync(trashItem.ApplicationId, folder.Id, cancellationToken);
        await trashRepository.DeleteAsync(trashItem.ApplicationId, trashItem.Id, cancellationToken);

        return Result<DeletePermanentlyResponse>.Success(new DeletePermanentlyResponse
        {
            ItemType = Contracts.Enumerations.TrashItemType.Folder,
            ItemId = folder.Id,
            ItemName = folder.Name.Value
        });
    }
}
