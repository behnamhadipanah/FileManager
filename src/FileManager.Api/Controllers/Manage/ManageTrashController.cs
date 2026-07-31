using FileManager.Application.Features.Queries.Trash;
using FileManager.Contracts.Responses.Trash;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers.Manage;

/// <summary>
/// Authorized trash browsing for the file manager admin UI.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = "Jwt")]
[Route("api/manage/applications/{applicationId:long}/trash")]
public sealed class ManageTrashController : BaseCqrsController
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
}
