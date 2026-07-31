using FileManager.Application.Features.Commands.Folders;
using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using FileManager.Domain.ValueObjects;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Folders;

public sealed class RenameFolderCommandHandler(IFolderRepository folderRepository)
    : IRequestHandler<RenameFolderCommand, FolderResponse>
{
    public async Task<Result<FolderResponse>> Handle(
        RenameFolderCommand command, CancellationToken cancellationToken = default)
    {
        var folder = await folderRepository.GetByBusinessIdAsync(
            command.ApplicationId, BusinessId.FromGuid(command.FolderBusinessId), cancellationToken);

        if (folder is null)
            return Result<FolderResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (UpdateFolderCommandHandler.IsRootFolder(folder))
            return Result<FolderResponse>.Failure(ResultStatus.ValidationError, DomainMessages.RootFolderProtected);

        if (folder.IsDeleted)
            return Result<FolderResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileAlreadyDeleted);

        var newName = FileName.FromString(command.Name.Trim());

        if (string.Equals(folder.Name.Value, newName.Value, StringComparison.Ordinal))
            return Result<FolderResponse>.Success(await FolderMapper.ToResponseAsync(folder, folderRepository, cancellationToken));

        if (await folderRepository.ExistsByNameAsync(
                command.ApplicationId, folder.ParentFolderId, newName.Value, cancellationToken))
            return Result<FolderResponse>.Failure(ResultStatus.Conflict, DomainMessages.FolderNameExists);

        var now = DateTime.UtcNow;
        folder.Rename(newName, now);
        await folderRepository.UpdateAsync(folder, cancellationToken);

        return Result<FolderResponse>.Success(
            await FolderMapper.ToResponseAsync(folder, folderRepository, cancellationToken));
    }
}
