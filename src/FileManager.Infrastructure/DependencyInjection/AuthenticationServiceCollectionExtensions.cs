using FileManager.Application.Authentication;
using FileManager.Domain.Aggregates.UserAgg;
using FileManager.Infrastructure.Authentication;
using FileManager.Infrastructure.Seeding;
using Kootam.Authentication.Abstractions.Claims;
using Kootam.Authentication.Abstractions.Services;
using Kootam.Authentication.DependencyInjection;
using Kootam.Authentication.Jwt.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Infrastructure.DependencyInjection;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddFileManagerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKootamAuthentication("Jwt")
            .UseCookies(options =>
            {
                options.AccessTokenKey = "fm_access_token";
                options.RefreshTokenKey = "fm_refresh_token";
                options.HttpOnly = true;
                options.Secure = false;
                options.SameSite = SameSiteMode.Lax;
            })
            .AddJwt(configuration);

        services.AddCurrentUser();
        services.AddScoped<IUserClaimsMapper<User>, UserClaimsMapper>();
        services.AddScoped<IRefreshTokenService<long>, SqlRefreshTokenService>();
        services.AddScoped<UserSeeder>();

        return services;
    }
}
