using FileManager.Application.Features.Commands.Folders;
using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Aggregates.TrashAgg;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Folders;

public sealed class DeleteFolderCommandHandler(
    IFolderRepository folderRepository,
    ITrashRepository trashRepository)
    : IRequestHandler<DeleteFolderCommand, DeleteFolderResponse>
{
    public async Task<Result<DeleteFolderResponse>> Handle(
        DeleteFolderCommand command, CancellationToken cancellationToken = default)
    {
        var folder = await folderRepository.GetByBusinessIdAsync(
            command.ApplicationId, BusinessId.FromGuid(command.FolderBusinessId), cancellationToken);

        if (folder is null)
            return Result<DeleteFolderResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (UpdateFolderCommandHandler.IsRootFolder(folder))
            return Result<DeleteFolderResponse>.Failure(ResultStatus.ValidationError, DomainMessages.RootFolderProtected);

        if (folder.IsDeleted)
            return Result<DeleteFolderResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileAlreadyDeleted);

        if (await trashRepository.GetByItemAsync(
                command.ApplicationId, TrashItemType.Folder, folder.Id, cancellationToken) is not null)
            return Result<DeleteFolderResponse>.Failure(ResultStatus.Conflict, DomainMessages.ItemAlreadyInTrash);

        if (await folderRepository.HasChildrenAsync(command.ApplicationId, folder.Id, cancellationToken))
            return Result<DeleteFolderResponse>.Failure(ResultStatus.ValidationError, DomainMessages.FolderNotEmpty);

        var now = DateTime.UtcNow;
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

        return Result<DeleteFolderResponse>.Success(new DeleteFolderResponse
        {
            BusinessId = (Guid)folder.BusinessId,
            Name = folder.Name.Value,
            TrashItemId = trashItem.Id
        });
    }
}
