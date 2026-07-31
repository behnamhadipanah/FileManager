using FileManager.Api.Authorization;
using FileManager.Application.Features.Queries.Folders;
using FileManager.Contracts.Responses.Folders;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// Folder browsing for client applications authenticated with an application token.
/// Send <c>X-Application-Token</c> from registration; omit folder id for root contents.
/// </summary>
[ApiController]
[ApplicationTokenAuth]
[Route("api/application/folders")]
public sealed class ApplicationFolderBrowseController : BaseCqrsController
{
    /// <summary>
    /// Lists child folders and files. Omit <paramref name="folderBusinessId"/> to browse the root folder.
    /// </summary>
    [HttpGet("contents")]
    [HttpGet("{folderBusinessId:guid}/contents")]
    public Task<IActionResult> GetContents(
        [FromRoute] Guid? folderBusinessId = null,
        [FromQuery] bool? isDeleted = null)
    {
        var query = new GetFolderContentsQuery(HttpContext.GetApplicationId(), folderBusinessId, isDeleted);
        return Query<FolderContentsResponse>(query);
    }
}
