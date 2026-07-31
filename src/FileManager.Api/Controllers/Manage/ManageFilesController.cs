using FileManager.Application.Abstractions;
using FileManager.Application.Features.Queries.Files;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Framework.Domain.ValueObjects;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers.Manage;

/// <summary>
/// Authorized file browsing for the file manager admin UI.
/// Public application APIs remain available without authentication.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = "Jwt")]
[Route("api/manage/applications/{applicationId:long}/files")]
public sealed class ManageFilesController(
    IApplicationRepository applicationRepository,
    IStorageFileRepository storageFileRepository,
    IFileStorageService fileStorageService) : BaseCqrsController
{
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
    /// Streams the generated thumbnail for an image or video file.
    /// </summary>
    [HttpGet("{fileBusinessId:guid}/thumbnail")]
    public async Task<IActionResult> GetThumbnail(
        [FromRoute] long applicationId,
        [FromRoute] Guid fileBusinessId,
        CancellationToken cancellationToken)
    {
        var fileResult = await ResolveFileAsync(applicationId, fileBusinessId, cancellationToken);
        if (fileResult.Error is not null)
            return fileResult.Error;

        var file = fileResult.File!;
        if (file.ThumbnailObjectKey is null || file.ThumbnailStatus != ThumbnailStatus.Completed)
            return NotFound(DomainMessages.FileNotFound);

        var stream = await fileStorageService.OpenThumbnailAsync(
            fileResult.StorageContext!,
            file.ThumbnailObjectKey.Value,
            cancellationToken);

        return File(stream, "image/webp");
    }

    /// <summary>
    /// Streams the stored file content. Used as a preview fallback for images.
    /// </summary>
    [HttpGet("{fileBusinessId:guid}/content")]
    public async Task<IActionResult> GetContent(
        [FromRoute] long applicationId,
        [FromRoute] Guid fileBusinessId,
        CancellationToken cancellationToken)
    {
        var fileResult = await ResolveFileAsync(applicationId, fileBusinessId, cancellationToken);
        if (fileResult.Error is not null)
            return fileResult.Error;

        var file = fileResult.File!;
        if (file.UploadStatus != UploadStatus.Completed)
            return NotFound(DomainMessages.FileNotFound);

        var stream = await fileStorageService.OpenFileAsync(
            fileResult.StorageContext!,
            file.ObjectKey.Value,
            cancellationToken);

        return File(stream, file.MimeType.Value);
    }

    private async Task<(Domain.Aggregates.FileAgg.StorageFile? File, ApplicationStorageContext? StorageContext, IActionResult? Error)> ResolveFileAsync(
        long applicationId,
        Guid fileBusinessId,
        CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetAsync(applicationId, cancellationToken);
        if (application is null)
            return (null, null, NotFound(DomainMessages.ApplicationNotFound));

        var file = await storageFileRepository.GetByBusinessIdAsync(
            applicationId, BusinessId.FromGuid(fileBusinessId), cancellationToken);

        if (file is null)
            return (null, null, NotFound(DomainMessages.FileNotFound));

        var storageContext = new ApplicationStorageContext(application.ApplicationName, file.FileType);
        return (file, storageContext, null);
    }
}
