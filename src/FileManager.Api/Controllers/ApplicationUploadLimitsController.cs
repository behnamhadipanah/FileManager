using FileManager.Application.Features.Queries.Applications;
using FileManager.Contracts.Responses.Applications;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// Upload limit settings loaded from the Applications table.
/// </summary>
[ApiController]
[Route("api/applications/{applicationId:long}/upload-limits")]
public sealed class ApplicationUploadLimitsController : BaseCqrsController
{
    /// <summary>
    /// Gets min/max upload limits (kilobytes) for images, videos, and documents.
    /// </summary>
    [HttpGet]
    public Task<IActionResult> Get([FromRoute] long applicationId)
    {
        var query = new GetApplicationUploadLimitsQuery(applicationId);
        return Query<ApplicationUploadLimitsResponse>(query);
    }
}
