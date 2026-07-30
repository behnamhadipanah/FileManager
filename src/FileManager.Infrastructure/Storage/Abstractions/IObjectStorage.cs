namespace FileManager.Infrastructure.Storage.Abstractions;

/// <summary>
/// Low-level, provider-agnostic object storage port. RustFS is the default
/// implementation (it speaks the S3 protocol), but any provider (MinIO, AWS S3,
/// Azure Blob, local disk, ...) can be swapped in by implementing this interface
/// and changing the DI registration - nothing above this layer needs to change.
/// </summary>
public interface IObjectStorage
{
    Task PutAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken cancellationToken);
    Task<Stream> GetAsync(string bucket, string objectKey, CancellationToken cancellationToken);
    Task DeleteAsync(string bucket, string objectKey, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string bucket, string objectKey, CancellationToken cancellationToken);
    Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken);
}
