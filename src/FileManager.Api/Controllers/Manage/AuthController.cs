using FileManager.Application.Features.Commands.Auth;
using FileManager.Contracts.Requests.Auth;
using FileManager.Contracts.Responses.Auth;
using Kootam.Framework.Presentations.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Api.Controllers.Manage;

/// <summary>
/// Authentication endpoints for the file manager admin UI.
/// Tokens are stored in HTTP-only cookies and also returned in the response body.
/// </summary>
[ApiController]
[Route("api/manage/auth")]
public sealed class AuthController : BaseCqrsController
{
    /// <summary>
    /// Authenticates a user and sets JWT access/refresh tokens in cookies.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password, request.RememberMe);
        return Create<LoginCommand, AuthTokensResponse>(command);
    }

    /// <summary>
    /// Rotates the refresh token and issues a new access token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public Task<IActionResult> Refresh()
        => Create<RefreshTokenCommand, AuthTokensResponse>(new RefreshTokenCommand());

    /// <summary>
    /// Revokes the current refresh token and clears auth cookies.
    /// </summary>
    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = "Jwt")]
    public Task<IActionResult> Logout()
        => Create<LogoutCommand, bool>(new LogoutCommand());
}
