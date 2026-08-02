namespace FileManager.Contracts.Responses.Applications;

public sealed class RegenerateApplicationTokenResponse
{
    public long ApplicationId { get; set; }
    public string Token { get; set; } = string.Empty;
}
