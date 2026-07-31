using FileManager.Application.Features.Commands.Auth;
using Kootam.Authentication.Abstractions.Services;
using Kootam.Authentication.Abstractions.Tokens;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Microsoft.AspNetCore.Http;

namespace FileManager.Application.Features.Commands.Auth;

public sealed class LogoutCommandHandler(
    IRefreshTokenReader refreshTokenReader,
    IRefreshTokenService<long> refreshTokenService,
    IAuthenticationService<long> authenticationService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<LogoutCommand, bool>
{
    public async Task<Result<bool>> Handle(
        LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            var refreshToken = await refreshTokenReader.ReadAsync(httpContext);
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await refreshTokenService.RevokeAsync(refreshToken, cancellationToken);
        }

        await authenticationService.SignOutAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
