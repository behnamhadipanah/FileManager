using FileManager.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FileManager.Api.Authorization;

/// <summary>
/// Validates the application token header and stores the resolved application id on the HTTP context.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApplicationTokenAuthAttribute : Attribute, IAsyncActionFilter
{
    public const string HeaderName = "X-Application-Token";
    public const string ApplicationIdItemKey = "ApplicationId";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var tokenValues)
            || string.IsNullOrWhiteSpace(tokenValues))
        {
            context.Result = new UnauthorizedObjectResult("Application token is required.");
            return;
        }

        var repository = context.HttpContext.RequestServices.GetRequiredService<IApplicationRepository>();
        var application = await repository.GetByTokenAsync(tokenValues.ToString(), context.HttpContext.RequestAborted);

        if (application is null || !application.IsActive)
        {
            context.Result = new UnauthorizedObjectResult("Invalid application token.");
            return;
        }

        context.HttpContext.Items[ApplicationIdItemKey] = application.Id;
        await next();
    }
}
