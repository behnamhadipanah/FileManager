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

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasher,
    ITokenGenerator<Domain.Aggregates.UserAgg.User, long> tokenGenerator,
    IRefreshTokenService<long> refreshTokenService,
    IAuthenticationService<long> authenticationService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<LoginCommand, AuthTokensResponse>
{
    public async Task<Result<AuthTokensResponse>> Handle(
        LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null || !user.IsActive)
            return Result<AuthTokensResponse>.Failure(ResultStatus.Unauthorized, "Invalid email or password.");

        if (!passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
            return Result<AuthTokensResponse>.Failure(ResultStatus.Unauthorized, "Invalid email or password.");

        var clientIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var accessToken = tokenGenerator.GenerateAccessToken(user);
        var refreshToken = tokenGenerator.GenerateRefreshToken(user.Id, clientIp ?? string.Empty);

        await refreshTokenService.StoreAsync(refreshToken, cancellationToken);

        var issuedToken = new IssuedToken<long>
        {
            AccessToken = accessToken.Token,
            AccessTokenExpires = accessToken.Expires,
            RefreshToken = refreshToken,
            RefreshTokenExpires = refreshToken.Expires
        };

        await authenticationService.SignInAsync(issuedToken, cancellationToken);

        return Result<AuthTokensResponse>.Success(new AuthTokensResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpires = accessToken.Expires,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpires = refreshToken.Expires
        });
    }
}
