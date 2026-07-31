using FileManager.Api.Authorization;
using FileManager.Application.Features.Queries.Applications;
using FileManager.Contracts.Responses.Applications;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// Upload limits for client applications authenticated with an application token.
/// </summary>
[ApiController]
[ApplicationTokenAuth]
[Route("api/application/upload-limits")]
public sealed class ApplicationUploadLimitsBrowseController : BaseCqrsController
{
    /// <summary>
    /// Gets min/max upload limits (kilobytes) for the authenticated application.
    /// </summary>
    [HttpGet]
    public Task<IActionResult> Get()
    {
        var query = new GetApplicationUploadLimitsQuery(HttpContext.GetApplicationId());
        return Query<ApplicationUploadLimitsResponse>(query);
    }
}
