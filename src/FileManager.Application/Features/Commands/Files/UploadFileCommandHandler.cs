using FileManager.Application.Services;
using FileManager.Application.Features.Commands.Folders;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Features.Commands.Files;

public sealed record UploadFileCommand(
    long ApplicationId,
    Guid? ParentFolderBusinessId,
    string FileName,
    string ContentType,
    Stream Content) : IRequest<StorageFileResponse>;

public sealed class UploadFileCommandHandler(
    IFolderRepository folderRepository,
    FileUploadService fileUploadService)
    : IRequestHandler<UploadFileCommand, StorageFileResponse>
{
    public async Task<Result<StorageFileResponse>> Handle(
        UploadFileCommand command, CancellationToken cancellationToken = default)
    {
        var (parent, errorStatus, errorMessage) = await FolderParentResolver.ResolveAsync(
            folderRepository, command.ApplicationId, command.ParentFolderBusinessId, cancellationToken);

        if (errorStatus is not null)
            return Result<StorageFileResponse>.Failure(errorStatus.Value, errorMessage!);

        return await fileUploadService.EnqueueUploadAsync(
            command.ApplicationId,
            parent!.Id,
            command.FileName,
            command.ContentType,
            command.Content,
            cancellationToken);
    }
}
