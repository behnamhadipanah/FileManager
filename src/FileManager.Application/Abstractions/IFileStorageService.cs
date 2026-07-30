namespace FileManager.Application.Abstractions;

/// <summary>
/// Application-level storage port. Keeps use cases decoupled from the concrete
/// object storage provider (RustFS today, swappable per the Infrastructure
/// IObjectStorage abstraction) and from bucket naming (files vs. thumbnails).
/// </summary>
public interface IFileStorageService
{
    Task SaveFileAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken);
    Task SaveThumbnailAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken);
    Task<Stream> OpenFileAsync(string objectKey, CancellationToken cancellationToken);
    Task<Stream> OpenThumbnailAsync(string objectKey, CancellationToken cancellationToken);
    Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken);
    Task DeleteThumbnailAsync(string objectKey, CancellationToken cancellationToken);
}
