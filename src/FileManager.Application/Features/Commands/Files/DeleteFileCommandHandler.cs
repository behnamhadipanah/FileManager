using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Aggregates.TrashAgg;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Files;

public sealed record DeleteFileCommand(
    long ApplicationId,
    Guid FileBusinessId) : IRequest<DeleteFileResponse>;

public sealed class DeleteFileCommandHandler(
    IStorageFileRepository storageFileRepository,
    ITrashRepository trashRepository)
    : IRequestHandler<DeleteFileCommand, DeleteFileResponse>
{
    public async Task<Result<DeleteFileResponse>> Handle(
        DeleteFileCommand command, CancellationToken cancellationToken = default)
    {
        var file = await storageFileRepository.GetByBusinessIdAsync(
            command.ApplicationId, BusinessId.FromGuid(command.FileBusinessId), cancellationToken);

        if (file is null)
            return Result<DeleteFileResponse>.Failure(ResultStatus.NotFound, DomainMessages.FileNotFound);

        if (file.IsDeleted)
            return Result<DeleteFileResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileAlreadyDeleted);

        if (await trashRepository.GetByItemAsync(
                command.ApplicationId, TrashItemType.File, file.Id, cancellationToken) is not null)
            return Result<DeleteFileResponse>.Failure(ResultStatus.Conflict, DomainMessages.ItemAlreadyInTrash);

        var now = DateTime.UtcNow;
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

        return Result<DeleteFileResponse>.Success(new DeleteFileResponse
        {
            BusinessId = (Guid)file.BusinessId,
            Name = file.Name.Value,
            TrashItemId = trashItem.Id
        });
    }
}
