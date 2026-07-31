using FileManager.Application.Features.Commands.Applications;
using FileManager.Contracts.Requests.Applications;
using FileManager.Contracts.Responses.Applications;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers;

/// <summary>
/// Registers applications that are allowed to store files in the file manager.
/// Each application is isolated from every other (multi-tenant by ApplicationId).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ApplicationsController : BaseCqrsController
{
    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] RegisterApplicationRequest request)
    {
        var command = new RegisterApplicationCommand(
            request.ApplicationName,
            request.MinSizeUploadImage,
            request.MaxSizeUploadImage,
            request.MinSizeVideo,
            request.MaxSizeVideo,
            request.MinSizeDcoument,
            request.MaxSizeDcoument);

        return Create<RegisterApplicationCommand, RegisterApplicationResponse>(command);
    }
}
