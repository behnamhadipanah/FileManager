using FileManager.Application.Features.Commands.Applications;
using FileManager.Application.Features.Queries.Applications;
using FileManager.Contracts.Responses.Applications;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers.Manage;

/// <summary>
/// Management endpoints for registered applications (admin menu).
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = "Jwt")]
[Route("api/manage/applications")]
public sealed class ManageApplicationsController : BaseCqrsController
{
    /// <summary>
    /// Returns active applications with token and upload limits.
    /// </summary>
    [HttpGet]
    public Task<IActionResult> GetAll()
        => Query<IReadOnlyList<ApplicationResponse>>(new GetApplicationsQuery());

    /// <summary>
    /// Gets min/max upload limits (kilobytes) for images, videos, and documents.
    /// </summary>
    [HttpGet("{applicationId:long}/upload-limits")]
    public Task<IActionResult> GetUploadLimits([FromRoute] long applicationId)
        => Query<ApplicationUploadLimitsResponse>(new GetApplicationUploadLimitsQuery(applicationId));

    /// <summary>
    /// Regenerates the application token. The previous token stops working immediately.
    /// </summary>
    [HttpPost("{applicationId:long}/regenerate-token")]
    public Task<IActionResult> RegenerateToken([FromRoute] long applicationId)
    {
        var command = new RegenerateApplicationTokenCommand(applicationId);
        return Create<RegenerateApplicationTokenCommand, RegenerateApplicationTokenResponse>(command);
    }
}
