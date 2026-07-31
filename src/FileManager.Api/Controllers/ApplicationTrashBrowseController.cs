using FileManager.Api.Authorization;
using FileManager.Application.Features.Queries.Trash;
using FileManager.Contracts.Responses.Trash;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// Trash browsing for client applications authenticated with an application token.
/// Send <c>X-Application-Token</c> from registration.
/// </summary>
[ApiController]
[ApplicationTokenAuth]
[Route("api/application/trash")]
public sealed class ApplicationTrashBrowseController : BaseCqrsController
{
    /// <summary>
    /// Lists trashed folders and files for the authenticated application.
    /// </summary>
    [HttpGet("items")]
    public Task<IActionResult> GetItems()
    {
        var query = new GetTrashContentsQuery(HttpContext.GetApplicationId());
        return Query<TrashContentsResponse>(query);
    }
}
