using FileManager.Application.Abstractions;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Files;

public sealed record DeleteFilePermanentlyCommand(
    long ApplicationId,
    Guid FileBusinessId) : IRequest<DeleteFilePermanentlyResponse>;

public sealed class DeleteFilePermanentlyCommandHandler(
    IStorageFileRepository storageFileRepository,
    ITrashRepository trashRepository,
    IFileStorageService fileStorageService)
    : IRequestHandler<DeleteFilePermanentlyCommand, DeleteFilePermanentlyResponse>
{
    public async Task<Result<DeleteFilePermanentlyResponse>> Handle(
        DeleteFilePermanentlyCommand command, CancellationToken cancellationToken = default)
    {
        var file = await storageFileRepository.GetByBusinessIdAsync(
            command.ApplicationId, BusinessId.FromGuid(command.FileBusinessId), cancellationToken);

        if (file is null)
            return Result<DeleteFilePermanentlyResponse>.Failure(ResultStatus.NotFound, DomainMessages.FileNotFound);

        var trashItem = await trashRepository.GetByItemAsync(
            command.ApplicationId, TrashItemType.File, file.Id, cancellationToken);

        await fileStorageService.DeleteFileAsync(file.ObjectKey.Value, cancellationToken);

        if (file.ThumbnailObjectKey is not null)
            await fileStorageService.DeleteThumbnailAsync(file.ThumbnailObjectKey.Value, cancellationToken);

        if (trashItem is not null)
            await trashRepository.DeleteAsync(command.ApplicationId, trashItem.Id, cancellationToken);

        await storageFileRepository.DeleteAsync(command.ApplicationId, file.Id, cancellationToken);

        return Result<DeleteFilePermanentlyResponse>.Success(new DeleteFilePermanentlyResponse
        {
            BusinessId = (Guid)file.BusinessId,
            Name = file.Name.Value
        });
    }
}
