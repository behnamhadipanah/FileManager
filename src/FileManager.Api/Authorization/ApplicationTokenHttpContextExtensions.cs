namespace FileManager.Api.Authorization;

internal static class ApplicationTokenHttpContextExtensions
{
    public static long GetApplicationId(this HttpContext httpContext) =>
        (long)httpContext.Items[ApplicationTokenAuthAttribute.ApplicationIdItemKey]!;
}
