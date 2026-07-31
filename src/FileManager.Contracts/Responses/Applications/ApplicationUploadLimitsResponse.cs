namespace FileManager.Contracts.Responses.Applications;

/// <summary>
/// Upload size limits for an application. All values are in kilobytes (KB).
/// </summary>
public sealed class ApplicationUploadLimitsResponse
{
    public long ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public long MinImageSizeKilobytes { get; set; }
    public long MaxImageSizeKilobytes { get; set; }
    public long MinVideoSizeKilobytes { get; set; }
    public long MaxVideoSizeKilobytes { get; set; }
    public long MinDocumentSizeKilobytes { get; set; }
    public long MaxDocumentSizeKilobytes { get; set; }
}
