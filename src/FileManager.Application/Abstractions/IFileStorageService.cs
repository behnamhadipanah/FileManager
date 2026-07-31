using FileManager.Application.Abstractions;

namespace FileManager.Application.Abstractions;

public interface IFileStorageService
{
    Task SaveFileAsync(
        ApplicationStorageContext context,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    Task SaveThumbnailAsync(
        ApplicationStorageContext context,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream> OpenFileAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken);

    Task<Stream> OpenThumbnailAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken);

    Task DeleteFileAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken);

    Task DeleteThumbnailAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken);
}
