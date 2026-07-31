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
    /// Returns active applications for the admin navigation menu.
    /// </summary>
    [HttpGet]
    public Task<IActionResult> GetAll()
        => Query<IReadOnlyList<ApplicationMenuItemResponse>>(new GetApplicationsQuery());
}
