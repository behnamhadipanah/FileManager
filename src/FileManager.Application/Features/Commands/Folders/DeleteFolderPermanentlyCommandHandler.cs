using FileManager.Application.Features.Commands.Folders;
using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Folders;

public sealed class DeleteFolderPermanentlyCommandHandler(
    IFolderRepository folderRepository,
    ITrashRepository trashRepository)
    : IRequestHandler<DeleteFolderPermanentlyCommand, DeleteFolderPermanentlyResponse>
{
    public async Task<Result<DeleteFolderPermanentlyResponse>> Handle(
        DeleteFolderPermanentlyCommand command, CancellationToken cancellationToken = default)
    {
        var folder = await folderRepository.GetByBusinessIdAsync(
            command.ApplicationId, BusinessId.FromGuid(command.FolderBusinessId), cancellationToken);

        if (folder is null)
            return Result<DeleteFolderPermanentlyResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (UpdateFolderCommandHandler.IsRootFolder(folder))
            return Result<DeleteFolderPermanentlyResponse>.Failure(ResultStatus.ValidationError, DomainMessages.RootFolderProtected);

        if (await folderRepository.HasChildrenAsync(command.ApplicationId, folder.Id, cancellationToken))
            return Result<DeleteFolderPermanentlyResponse>.Failure(ResultStatus.ValidationError, DomainMessages.FolderNotEmpty);

        var trashItem = await trashRepository.GetByItemAsync(
            command.ApplicationId, TrashItemType.Folder, folder.Id, cancellationToken);

        if (trashItem is not null)
            await trashRepository.DeleteAsync(command.ApplicationId, trashItem.Id, cancellationToken);

        await folderRepository.DeleteAsync(command.ApplicationId, folder.Id, cancellationToken);

        return Result<DeleteFolderPermanentlyResponse>.Success(new DeleteFolderPermanentlyResponse
        {
            BusinessId = (Guid)folder.BusinessId,
            Name = folder.Name.Value
        });
    }
}
