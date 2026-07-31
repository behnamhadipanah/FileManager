using FileManager.Application.Features.Commands.Folders;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Files;

public sealed record RestoreFileCommand(
    long ApplicationId,
    Guid FileBusinessId) : IRequest<RestoreFileResponse>;

public sealed class RestoreFileCommandHandler(
    IFolderRepository folderRepository,
    IStorageFileRepository storageFileRepository,
    ITrashRepository trashRepository)
    : IRequestHandler<RestoreFileCommand, RestoreFileResponse>
{
    public async Task<Result<RestoreFileResponse>> Handle(
        RestoreFileCommand command, CancellationToken cancellationToken = default)
    {
        var file = await storageFileRepository.GetByBusinessIdAsync(
            command.ApplicationId, BusinessId.FromGuid(command.FileBusinessId), cancellationToken);

        if (file is null)
            return Result<RestoreFileResponse>.Failure(ResultStatus.NotFound, DomainMessages.FileNotFound);

        if (!file.IsDeleted)
            return Result<RestoreFileResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileNotInTrash);

        var trashItem = await trashRepository.GetByItemAsync(
            command.ApplicationId, TrashItemType.File, file.Id, cancellationToken);
        if (trashItem is null)
            return Result<RestoreFileResponse>.Failure(ResultStatus.NotFound, DomainMessages.TrashItemNotFound);

        if (trashItem.IsPurged)
            return Result<RestoreFileResponse>.Failure(ResultStatus.Conflict, DomainMessages.TrashItemAlreadyPurged);

        if (trashItem.IsRestored)
            return Result<RestoreFileResponse>.Failure(ResultStatus.Conflict, DomainMessages.TrashItemAlreadyRestored);

        var now = DateTime.UtcNow;
        var (restoredParentId, errorStatus, errorMessage) = await FolderHierarchyRestorer.EnsureParentFolderChainAsync(
            folderRepository, trashRepository, command.ApplicationId, trashItem.OriginalParentFolderId, now, cancellationToken);

        if (errorStatus is not null)
            return Result<RestoreFileResponse>.Failure(errorStatus.Value, errorMessage!);

        if (file.ParentFolderId != restoredParentId)
            file.Move(restoredParentId, now);

        if (await storageFileRepository.ExistsByNameAsync(
                command.ApplicationId, restoredParentId, file.Name.Value, cancellationToken))
            return Result<RestoreFileResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileNameExists);

        file.Restore();
        trashItem.Restore(now);

        await storageFileRepository.UpdateAsync(file, cancellationToken);
        await trashRepository.UpdateAsync(trashItem, cancellationToken);

        return Result<RestoreFileResponse>.Success(new RestoreFileResponse
        {
            BusinessId = (Guid)file.BusinessId,
            Name = file.Name.Value,
            RestoredToFolderId = restoredParentId
        });
    }
}
