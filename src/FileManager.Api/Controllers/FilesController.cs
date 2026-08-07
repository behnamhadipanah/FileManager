using FileManager.Application.Abstractions;
using FileManager.Application.Features.Commands.Files;
using FileManager.Application.Features.Queries.Files;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Framework.Domain.ValueObjects;
using Kootam.Framework.Presentations.Controllers;
using Kootam.Translator.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// CRUD operations for storage files within an application scope.
/// </summary>
[ApiController]
[Route("api/applications/{applicationId:long}/files")]
[RequestSizeLimit(524_288_000)]
[RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
public sealed class FilesController(
    IApplicationRepository applicationRepository,
    IStorageFileRepository storageFileRepository,
    IFileStorageService fileStorageService,
    ITranslator translator) : BaseCqrsController
{
    /// <summary>
    /// Queues a file upload for background processing. Returns immediately with UploadStatus Pending (0).
    /// Poll GET until UploadStatus is Completed (2). Images (except SVG) convert to WebP; videos convert to WebM.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StorageFileResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Upload(
        [FromRoute] long applicationId,
        [FromForm] IFormFile file,
        [FromForm] Guid? parentFolderBusinessId = null)
    {
        await using var stream = file.OpenReadStream();
        var command = new UploadFileCommand(
            applicationId,
            parentFolderBusinessId,
            file.FileName,
            file.ContentType,
            stream);

        return await Create<UploadFileCommand, StorageFileResponse>(command);
    }

    /// <summary>
    /// Queues multiple files for background processing. Returns accepted files with UploadStatus Pending (0).
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(UploadFilesResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> UploadBatch(
        [FromRoute] long applicationId,
        [FromForm] IFormFileCollection files,
        [FromForm] Guid? parentFolderBusinessId = null)
    {
        var payloads = new List<UploadFilePayload>(files.Count);
        foreach (var file in files)
        {
            payloads.Add(new UploadFilePayload(
                file.FileName,
                file.ContentType,
                file.OpenReadStream()));
        }

        try
        {
            var command = new UploadFilesCommand(applicationId, parentFolderBusinessId, payloads);
            return await Create<UploadFilesCommand, UploadFilesResponse>(command);
        }
        finally
        {
            foreach (var payload in payloads)
                await payload.Content.DisposeAsync();
        }
    }

    /// <summary>
    /// Gets a file by business id. Optional isDeleted filter.
    /// </summary>
    [HttpGet("{fileBusinessId:guid}")]
    public Task<IActionResult> Get(
        [FromRoute] long applicationId,
        [FromRoute] Guid fileBusinessId,
        [FromQuery] bool? isDeleted = null)
    {
        var query = new GetFileQuery(applicationId, fileBusinessId, isDeleted);
        return Query<StorageFileResponse>(query);
    }

    /// <summary>
    /// Downloads the stored file content as an attachment.
    /// </summary>
    [HttpGet("{fileBusinessId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        [FromRoute] long applicationId,
        [FromRoute] Guid fileBusinessId,
        CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetAsync(applicationId, cancellationToken);
        if (application is null)
            return NotFound(translator.Get(DomainMessages.ApplicationNotFound));

        var file = await storageFileRepository.GetByBusinessIdAsync(
            applicationId, BusinessId.FromGuid(fileBusinessId), cancellationToken);

        if (file is null || file.IsDeleted || file.UploadStatus != UploadStatus.Completed)
            return NotFound(translator.Get(DomainMessages.FileNotFound));

        var stream = await fileStorageService.OpenFileAsync(
            new ApplicationStorageContext(application.ApplicationName, file.FileType),
            file.ObjectKey.Value,
            cancellationToken);

        return File(stream, file.MimeType.Value, file.Name.Value);
    }

    /// <summary>
    /// Soft-deletes a file by moving it to trash.
    /// </summary>
    [HttpDelete("{fileBusinessId:guid}")]
    public Task<IActionResult> Delete(
        [FromRoute] long applicationId,
        [FromRoute] Guid fileBusinessId)
    {
        var command = new DeleteFileCommand(applicationId, fileBusinessId);
        return Create<DeleteFileCommand, DeleteFileResponse>(command);
    }

    /// <summary>
    /// Permanently deletes a file from storage and the database.
    /// </summary>
    [HttpDelete("{fileBusinessId:guid}/permanent")]
    public Task<IActionResult> DeletePermanently(
        [FromRoute] long applicationId,
        [FromRoute] Guid fileBusinessId)
    {
        var command = new DeleteFilePermanentlyCommand(applicationId, fileBusinessId);
        return Create<DeleteFilePermanentlyCommand, DeleteFilePermanentlyResponse>(command);
    }

    /// <summary>
    /// Restores a trashed file, recreating missing parent folders in the chain when needed.
    /// </summary>
    [HttpPost("{fileBusinessId:guid}/restore")]
    public Task<IActionResult> Restore(
        [FromRoute] long applicationId,
        [FromRoute] Guid fileBusinessId)
    {
        var command = new RestoreFileCommand(applicationId, fileBusinessId);
        return Create<RestoreFileCommand, RestoreFileResponse>(command);
    }
}
