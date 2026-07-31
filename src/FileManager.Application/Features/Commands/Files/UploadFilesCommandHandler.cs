using FileManager.Application.Services;
using FileManager.Application.Features.Commands.Folders;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Features.Commands.Files;

public sealed record UploadFilePayload(string FileName, string ContentType, Stream Content);

public sealed record UploadFilesCommand(
    long ApplicationId,
    Guid? ParentFolderBusinessId,
    IReadOnlyList<UploadFilePayload> Files) : IRequest<UploadFilesResponse>;

public sealed class UploadFilesCommandHandler(
    IFolderRepository folderRepository,
    FileUploadService fileUploadService)
    : IRequestHandler<UploadFilesCommand, UploadFilesResponse>
{
    public async Task<Result<UploadFilesResponse>> Handle(
        UploadFilesCommand command, CancellationToken cancellationToken = default)
    {
        var (parent, errorStatus, errorMessage) = await FolderParentResolver.ResolveAsync(
            folderRepository, command.ApplicationId, command.ParentFolderBusinessId, cancellationToken);

        if (errorStatus is not null)
            return Result<UploadFilesResponse>.Failure(errorStatus.Value, errorMessage!);

        var uploaded = new List<StorageFileResponse>();
        var errors = new List<UploadFileError>();

        foreach (var file in command.Files)
        {
            var result = await fileUploadService.EnqueueUploadAsync(
                command.ApplicationId,
                parent!.Id,
                file.FileName,
                file.ContentType,
                file.Content,
                cancellationToken);

            if (result.IsSuccess)
                uploaded.Add(result.Data!);
            else
                errors.Add(new UploadFileError { FileName = file.FileName, ErrorCode = "upload_failed" });
        }

        return Result<UploadFilesResponse>.Success(new UploadFilesResponse
        {
            Files = uploaded,
            Errors = errors
        });
    }
}
