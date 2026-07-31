namespace FileManager.Contracts.Responses.Auth;

public sealed class AuthTokensResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset AccessTokenExpires { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }
}
