namespace FileManager.Contracts.Responses.Applications;

public sealed class ApplicationMenuItemResponse
{
    public long Id { get; set; }
    public Guid BusinessId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
}
