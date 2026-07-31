namespace FileManager.Contracts.Responses.Files;

public sealed class UploadFilesResponse
{
    public IReadOnlyList<StorageFileResponse> Files { get; set; } = [];
    public IReadOnlyList<UploadFileError> Errors { get; set; } = [];
}

public sealed class UploadFileError
{
    public string FileName { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}
