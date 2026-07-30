namespace FileManager.Contracts.Requests.Applications;

public sealed class RegisterApplicationRequest
{
    public string ApplicationName { get; set; } = string.Empty;
    public long MinSizeUploadImage { get; set; }
    public long MaxSizeUploadImage { get; set; }
    public long MinSizeVideo { get; set; }
    public long MaxSizeVideo { get; set; }
    public long MinSizeDcoument { get; set; }
    public long MaxSizeDcoument { get; set; }
}
