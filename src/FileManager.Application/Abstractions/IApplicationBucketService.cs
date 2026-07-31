using FileManager.Domain.Enumerations;

namespace FileManager.Application.Abstractions;

public sealed record ApplicationStorageContext(string ApplicationName, StorageFileType FileType);

public interface IApplicationBucketService
{
    Task EnsureApplicationBucketsAsync(string applicationName, CancellationToken cancellationToken = default);
    Task EnsureBucketsExistAsync(string applicationName, StorageFileType fileType, CancellationToken cancellationToken = default);
}
