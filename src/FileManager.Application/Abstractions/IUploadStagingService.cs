namespace FileManager.Application.Abstractions;

public interface IUploadStagingService
{
    Task<string> SaveAsync(Stream content, CancellationToken cancellationToken);
    Stream OpenRead(string stagingPath);
    Task DeleteAsync(string stagingPath, CancellationToken cancellationToken);
}
