using FileManager.Api.Authorization;
using FileManager.Application.Features.Commands.Applications;
using FileManager.Application.Features.Queries.Applications;
using FileManager.Contracts.Responses.Applications;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// Application metadata for clients authenticated with an application token.
/// Send <c>X-Application-Token</c> from registration.
/// </summary>
[ApiController]
[ApplicationTokenAuth]
[Route("api/application")]
public sealed class ApplicationBrowseController : BaseCqrsController
{
    /// <summary>
    /// Returns the application bound to the provided application token.
    /// </summary>
    [HttpGet]
    public Task<IActionResult> Get()
    {
        var query = new GetApplicationQuery(HttpContext.GetApplicationId());
        return Query<ApplicationResponse>(query);
    }

    /// <summary>
    /// Regenerates the application token. The previous token stops working immediately.
    /// </summary>
    [HttpPost("regenerate-token")]
    public Task<IActionResult> RegenerateToken()
    {
        var command = new RegenerateApplicationTokenCommand(HttpContext.GetApplicationId());
        return Create<RegenerateApplicationTokenCommand, RegenerateApplicationTokenResponse>(command);
    }
}
