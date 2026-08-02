using FileManager.Contracts.Responses.Applications;
using FileManager.Domain.Aggregates.ApplicationAgg;

namespace FileManager.Application.Mappers;

internal static class ApplicationMapper
{
    public static ApplicationResponse ToResponse(RegisteredApplication application) =>
        new()
        {
            Id = application.Id,
            Name = application.ApplicationName,
            BusinessId = (Guid)application.BusinessId,
            Token = application.Token.Value,
            MinImageSizeKilobytes = application.UploadLimits.MinImageSizeKilobytes,
            MaxImageSizeKilobytes = application.UploadLimits.MaxImageSizeKilobytes,
            MinVideoSizeKilobytes = application.UploadLimits.MinVideoSizeKilobytes,
            MaxVideoSizeKilobytes = application.UploadLimits.MaxVideoSizeKilobytes,
            MinDocumentSizeKilobytes = application.UploadLimits.MinDocumentSizeKilobytes,
            MaxDocumentSizeKilobytes = application.UploadLimits.MaxDocumentSizeKilobytes
        };

    public static ApplicationUploadLimitsResponse ToUploadLimitsResponse(RegisteredApplication application) =>
        new()
        {
            ApplicationId = application.Id,
            ApplicationName = application.ApplicationName,
            MinImageSizeKilobytes = application.UploadLimits.MinImageSizeKilobytes,
            MaxImageSizeKilobytes = application.UploadLimits.MaxImageSizeKilobytes,
            MinVideoSizeKilobytes = application.UploadLimits.MinVideoSizeKilobytes,
            MaxVideoSizeKilobytes = application.UploadLimits.MaxVideoSizeKilobytes,
            MinDocumentSizeKilobytes = application.UploadLimits.MinDocumentSizeKilobytes,
            MaxDocumentSizeKilobytes = application.UploadLimits.MaxDocumentSizeKilobytes
        };
}
