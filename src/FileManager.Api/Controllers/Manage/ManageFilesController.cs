using FileManager.Application.Features.Queries.Files;
using FileManager.Contracts.Responses.Files;
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
public sealed class ManageFilesController : BaseCqrsController
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
}
