namespace FileManager.Contracts.Responses.Files;

public sealed class DeleteFilePermanentlyResponse
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
}
