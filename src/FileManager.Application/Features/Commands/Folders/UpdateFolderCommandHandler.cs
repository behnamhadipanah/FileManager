using FileManager.Application.Features.Commands.Folders;
using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Aggregates.FolderAgg;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Folders;

public sealed class UpdateFolderCommandHandler(IFolderRepository folderRepository)
    : IRequestHandler<UpdateFolderCommand, FolderResponse>
{
    public async Task<Result<FolderResponse>> Handle(
        UpdateFolderCommand command, CancellationToken cancellationToken = default)
    {
        var folder = await folderRepository.GetByBusinessIdAsync(
            command.ApplicationId, BusinessId.FromGuid(command.FolderBusinessId), cancellationToken);

        if (folder is null)
            return Result<FolderResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (IsRootFolder(folder))
            return Result<FolderResponse>.Failure(ResultStatus.ValidationError, DomainMessages.RootFolderProtected);

        if (folder.IsDeleted)
            return Result<FolderResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileAlreadyDeleted);

        var (parent, errorStatus, errorMessage) = await FolderParentResolver.ResolveAsync(
            folderRepository, command.ApplicationId, command.ParentFolderBusinessId, cancellationToken);

        if (errorStatus is not null)
            return Result<FolderResponse>.Failure(errorStatus.Value, errorMessage!);

        if (parent!.Id == folder.Id)
            return Result<FolderResponse>.Failure(ResultStatus.ValidationError, DomainMessages.CircularFolderReference);

        if (parent.Id == folder.ParentFolderId)
            return Result<FolderResponse>.Success(
                FolderMapper.ToResponse(folder, parent.BusinessId));

        if (await folderRepository.ExistsByNameAsync(
                command.ApplicationId, parent.Id, folder.Name.Value, cancellationToken))
            return Result<FolderResponse>.Failure(ResultStatus.Conflict, DomainMessages.FolderNameExists);

        var now = DateTime.UtcNow;
        folder.Move(parent.Id, now);
        await folderRepository.UpdateAsync(folder, cancellationToken);

        return Result<FolderResponse>.Success(
            FolderMapper.ToResponse(folder, parent.BusinessId));
    }

    internal static bool IsRootFolder(Folder folder) =>
        folder.ParentFolderId is null && folder.Name.Value == Folder.RootFolderName;
}
