namespace FileManager.Contracts.Responses.Applications;

public sealed class RegisterApplicationResponse
{
    public long ApplicationId { get; set; }
    public string Token { get; set; } = string.Empty;
    public long RootFolderId { get; set; }
}
