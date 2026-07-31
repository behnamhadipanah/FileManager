using FileManager.Application.Features.Queries.Folders;
using FileManager.Contracts.Responses.Folders;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers.Manage;

/// <summary>
/// Authorized folder browsing for the file manager admin UI.
/// Public application APIs remain available without authentication.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = "Jwt")]
[Route("api/manage/applications/{applicationBusinessId:guid}/folders")]
public sealed class ManageFoldersController : BaseCqrsController
{
    /// <summary>
    /// Gets a folder by business id, or the root folder when <paramref name="folderBusinessId"/> is omitted.
    /// Optional isDeleted filter (0 = active, 1 = deleted).
    /// </summary>
    [HttpGet]
    [HttpGet("{folderBusinessId:guid}")]
    public Task<IActionResult> Get(
        [FromRoute] Guid applicationBusinessId,
        [FromRoute] Guid? folderBusinessId = null,
        [FromQuery] bool? isDeleted = null)
    {
        var query = new GetManageFolderQuery(applicationBusinessId, folderBusinessId, isDeleted);
        return Query<FolderResponse>(query);
    }
}
