using FileManager.Application.Features.Commands.Folders;
using FileManager.Application.Features.Queries.Folders;
using FileManager.Contracts.Requests.Folders;
using FileManager.Contracts.Responses.Folders;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// CRUD operations for folders within an application scope.
/// </summary>
[ApiController]
[Route("api/applications/{applicationId:long}/folders")]
public sealed class FoldersController : BaseCqrsController
{
    /// <summary>
    /// Creates a folder. When <paramref name="request"/>.ParentFolderBusinessId is omitted, the folder is created under the root folder.
    /// </summary>
    [HttpPost]
    public Task<IActionResult> Create(
        [FromRoute] long applicationId,
        [FromBody] CreateFolderRequest request)
    {
        var command = new CreateFolderCommand(applicationId, request.Name, request.ParentFolderBusinessId);
        return Create<CreateFolderCommand, FolderResponse>(command);
    }

    /// <summary>
    /// Gets a folder by business id. Optional <paramref name="isDeleted"/> filter (0 = active, 1 = deleted).
    /// </summary>
    [HttpGet("{folderBusinessId:guid}")]
    public Task<IActionResult> Get(
        [FromRoute] long applicationId,
        [FromRoute] Guid folderBusinessId,
        [FromQuery] bool? isDeleted = null)
    {
        var query = new GetFolderQuery(applicationId, folderBusinessId, isDeleted);
        return Query<FolderResponse>(query);
    }

    /// <summary>
    /// Moves a folder to a new parent. When ParentFolderBusinessId is omitted, moves under the root folder.
    /// </summary>
    [HttpPut("{folderBusinessId:guid}")]
    public Task<IActionResult> Update(
        [FromRoute] long applicationId,
        [FromRoute] Guid folderBusinessId,
        [FromBody] UpdateFolderRequest request)
    {
        var command = new UpdateFolderCommand(applicationId, folderBusinessId, request.ParentFolderBusinessId);
        return Create<UpdateFolderCommand, FolderResponse>(command);
    }

    /// <summary>
    /// Renames a folder.
    /// </summary>
    [HttpPatch("{folderBusinessId:guid}/name")]
    public Task<IActionResult> Rename(
        [FromRoute] long applicationId,
        [FromRoute] Guid folderBusinessId,
        [FromBody] RenameFolderRequest request)
    {
        var command = new RenameFolderCommand(applicationId, folderBusinessId, request.Name);
        return Create<RenameFolderCommand, FolderResponse>(command);
    }

    /// <summary>
    /// Soft-deletes a folder by moving it to trash.
    /// </summary>
    [HttpDelete("{folderBusinessId:guid}")]
    public Task<IActionResult> Delete(
        [FromRoute] long applicationId,
        [FromRoute] Guid folderBusinessId)
    {
        var command = new DeleteFolderCommand(applicationId, folderBusinessId);
        return Create<DeleteFolderCommand, DeleteFolderResponse>(command);
    }

    /// <summary>
    /// Permanently deletes a folder without going through trash.
    /// </summary>
    [HttpDelete("{folderBusinessId:guid}/permanent")]
    public Task<IActionResult> DeletePermanently(
        [FromRoute] long applicationId,
        [FromRoute] Guid folderBusinessId)
    {
        var command = new DeleteFolderPermanentlyCommand(applicationId, folderBusinessId);
        return Create<DeleteFolderPermanentlyCommand, DeleteFolderPermanentlyResponse>(command);
    }
}
