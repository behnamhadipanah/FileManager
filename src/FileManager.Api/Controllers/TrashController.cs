using FileManager.Application.Features.Commands.Trash;
using FileManager.Application.Features.Queries.Trash;
using FileManager.Contracts.Requests.Trash;
using FileManager.Contracts.Responses.Trash;
using FileManager.Domain.Enumerations;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// Manages trash operations: move items to trash, restore, and permanent deletion.
/// Supports both files and folders within an application scope.
/// </summary>
[ApiController]
[Route("api/applications/{applicationId:long}/trash")]
public sealed class TrashController : BaseCqrsController
{
    /// <summary>
    /// Lists trashed folders and files for the application.
    /// </summary>
    [HttpGet("items")]
    public Task<IActionResult> GetItems([FromRoute] long applicationId)
    {
        var query = new GetTrashContentsQuery(applicationId);
        return Query<TrashContentsResponse>(query);
    }

    /// <summary>
    /// Moves a file or folder to trash (soft delete).
    /// </summary>
    [HttpPost("items")]
    public Task<IActionResult> MoveToTrash(
        [FromRoute] long applicationId,
        [FromBody] MoveToTrashRequest request)
    {
        var command = new MoveToTrashCommand(
            applicationId,
            (TrashItemType)request.ItemType,
            request.ItemId);

        return Create<MoveToTrashCommand, MoveToTrashResponse>(command);
    }

    /// <summary>
    /// Restores a trashed file or folder to its original location.
    /// </summary>
    [HttpPost("items/{trashItemId:long}/restore")]
    public Task<IActionResult> Restore(
        [FromRoute] long applicationId,
        [FromRoute] long trashItemId)
    {
        var command = new RestoreTrashItemCommand(applicationId, trashItemId);
        return Create<RestoreTrashItemCommand, RestoreTrashItemResponse>(command);
    }

    /// <summary>
    /// Permanently deletes a trashed file or folder from the database (and storage for files).
    /// </summary>
    [HttpDelete("items/{trashItemId:long}")]
    public Task<IActionResult> DeletePermanently(
        [FromRoute] long applicationId,
        [FromRoute] long trashItemId)
    {
        var command = new DeletePermanentlyCommand(applicationId, trashItemId);
        return Create<DeletePermanentlyCommand, DeletePermanentlyResponse>(command);
    }
}
