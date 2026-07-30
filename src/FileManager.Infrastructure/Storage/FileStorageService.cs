using FileManager.Application.Abstractions;
using FileManager.Infrastructure.Configuration;
using FileManager.Infrastructure.Storage.Abstractions;
using Microsoft.Extensions.Options;

namespace FileManager.Infrastructure.Storage;

public sealed class FileStorageService(IObjectStorage objectStorage, IOptions<RustFsOptions> options) : IFileStorageService
{
    private readonly RustFsOptions _options = options.Value;

    public Task SaveFileAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken) =>
        objectStorage.PutAsync(_options.FilesBucket, objectKey, content, contentType, cancellationToken);

    public Task SaveThumbnailAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken) =>
        objectStorage.PutAsync(_options.ThumbnailsBucket, objectKey, content, contentType, cancellationToken);

    public Task<Stream> OpenFileAsync(string objectKey, CancellationToken cancellationToken) =>
        objectStorage.GetAsync(_options.FilesBucket, objectKey, cancellationToken);

    public Task<Stream> OpenThumbnailAsync(string objectKey, CancellationToken cancellationToken) =>
        objectStorage.GetAsync(_options.ThumbnailsBucket, objectKey, cancellationToken);

    public Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken) =>
        objectStorage.DeleteAsync(_options.FilesBucket, objectKey, cancellationToken);

    public Task DeleteThumbnailAsync(string objectKey, CancellationToken cancellationToken) =>
        objectStorage.DeleteAsync(_options.ThumbnailsBucket, objectKey, cancellationToken);
}
