namespace FileManager.Contracts.Responses.Applications;

/// <summary>
/// Application details including token and upload size limits (kilobytes).
/// </summary>
public sealed class ApplicationResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid BusinessId { get; set; }
    public string Token { get; set; } = string.Empty;
    public long MinImageSizeKilobytes { get; set; }
    public long MaxImageSizeKilobytes { get; set; }
    public long MinVideoSizeKilobytes { get; set; }
    public long MaxVideoSizeKilobytes { get; set; }
    public long MinDocumentSizeKilobytes { get; set; }
    public long MaxDocumentSizeKilobytes { get; set; }
}
