using FileManager.Application.Features.Commands.Auth;
using FileManager.Contracts.Responses.Auth;
using FileManager.Domain.Repositories;
using Kootam.Authentication.Abstractions.Models;
using Kootam.Authentication.Abstractions.Services;
using Kootam.Authentication.Abstractions.Tokens;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Microsoft.AspNetCore.Http;

namespace FileManager.Application.Features.Commands.Auth;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenReader refreshTokenReader,
    ITokenGenerator<Domain.Aggregates.UserAgg.User, long> tokenGenerator,
    IRefreshTokenService<long> refreshTokenService,
    IAuthenticationService<long> authenticationService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<RefreshTokenCommand, AuthTokensResponse>
{
    public async Task<Result<AuthTokensResponse>> Handle(
        RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return Result<AuthTokensResponse>.Failure(ResultStatus.Unauthorized, "Refresh token is missing.");

        var currentToken = await refreshTokenReader.ReadAsync(httpContext);
        if (string.IsNullOrWhiteSpace(currentToken))
            return Result<AuthTokensResponse>.Failure(ResultStatus.Unauthorized, "Refresh token is missing.");

        var storedToken = await refreshTokenService.FindAsync(currentToken, cancellationToken);
        if (storedToken is null || !storedToken.IsActive)
            return Result<AuthTokensResponse>.Failure(ResultStatus.Unauthorized, "Refresh token is invalid or expired.");

        var user = await userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            return Result<AuthTokensResponse>.Failure(ResultStatus.Unauthorized, "User is not available.");

        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        var accessToken = tokenGenerator.GenerateAccessToken(user);
        var replacement = tokenGenerator.GenerateRefreshToken(user.Id, clientIp ?? string.Empty);

        await refreshTokenService.RotateAsync(storedToken, replacement, cancellationToken);

        var issuedToken = new IssuedToken<long>
        {
            AccessToken = accessToken.Token,
            AccessTokenExpires = accessToken.Expires,
            RefreshToken = replacement,
            RefreshTokenExpires = replacement.Expires
        };

        await authenticationService.SignInAsync(issuedToken, cancellationToken);

        return Result<AuthTokensResponse>.Success(new AuthTokensResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpires = accessToken.Expires,
            RefreshToken = replacement.Token,
            RefreshTokenExpires = replacement.Expires
        });
    }
}
