using FileManager.Application.Features.Commands.Trash;
using FileManager.Domain.Aggregates.TrashAgg;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using FileManager.Contracts.Responses.Trash;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Features.Commands.Trash;

public sealed class MoveToTrashCommandHandler(
    IFolderRepository folderRepository,
    IStorageFileRepository storageFileRepository,
    ITrashRepository trashRepository)
    : IRequestHandler<MoveToTrashCommand, MoveToTrashResponse>
{
    public async Task<Result<MoveToTrashResponse>> Handle(
        MoveToTrashCommand command, CancellationToken cancellationToken = default)
    {
        if (await trashRepository.GetByItemAsync(command.ApplicationId, command.ItemType, command.ItemId, cancellationToken) is not null)
            return Result<MoveToTrashResponse>.Failure(ResultStatus.Conflict, DomainMessages.ItemAlreadyInTrash);

        var now = DateTime.UtcNow;

        return command.ItemType switch
        {
            TrashItemType.File => await MoveFileToTrashAsync(command, now, cancellationToken),
            TrashItemType.Folder => await MoveFolderToTrashAsync(command, now, cancellationToken),
            _ => Result<MoveToTrashResponse>.Failure(ResultStatus.ValidationError, DomainMessages.UnsupportedFileType)
        };
    }

    private async Task<Result<MoveToTrashResponse>> MoveFileToTrashAsync(
        MoveToTrashCommand command, DateTime now, CancellationToken cancellationToken)
    {
        var file = await storageFileRepository.GetAsync(command.ApplicationId, command.ItemId, cancellationToken);
        if (file is null)
            return Result<MoveToTrashResponse>.Failure(ResultStatus.NotFound, DomainMessages.FileNotFound);

        if (file.IsDeleted)
            return Result<MoveToTrashResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileAlreadyDeleted);

        file.Delete(now);

        var trashItem = TrashItem.Create(
            command.ApplicationId,
            TrashItemType.File,
            file.Id,
            file.Name.Value,
            file.ParentFolderId,
            now);

        await storageFileRepository.UpdateAsync(file, cancellationToken);
        await trashRepository.InsertAsync(trashItem, cancellationToken);

        return Result<MoveToTrashResponse>.Success(new MoveToTrashResponse
        {
            TrashItemId = trashItem.Id,
            ItemType = Contracts.Enumerations.TrashItemType.File,
            ItemId = file.Id,
            ItemName = file.Name.Value
        });
    }

    private async Task<Result<MoveToTrashResponse>> MoveFolderToTrashAsync(
        MoveToTrashCommand command, DateTime now, CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetAsync(command.ApplicationId, command.ItemId, cancellationToken);
        if (folder is null)
            return Result<MoveToTrashResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (folder.IsDeleted)
            return Result<MoveToTrashResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileAlreadyDeleted);

        if (await folderRepository.HasChildrenAsync(command.ApplicationId, folder.Id, cancellationToken))
            return Result<MoveToTrashResponse>.Failure(ResultStatus.ValidationError, DomainMessages.FolderNotEmpty);

        folder.Delete(now);

        var trashItem = TrashItem.Create(
            command.ApplicationId,
            TrashItemType.Folder,
            folder.Id,
            folder.Name.Value,
            folder.ParentFolderId,
            now);

        await folderRepository.UpdateAsync(folder, cancellationToken);
        await trashRepository.InsertAsync(trashItem, cancellationToken);

        return Result<MoveToTrashResponse>.Success(new MoveToTrashResponse
        {
            TrashItemId = trashItem.Id,
            ItemType = Contracts.Enumerations.TrashItemType.Folder,
            ItemId = folder.Id,
            ItemName = folder.Name.Value
        });
    }
}
