using FileManager.Application.Abstractions;

namespace FileManager.Application.Abstractions;

public interface IUploadStagingService
{
    Task<StagingUploadResult> SaveAsync(
        ApplicationStorageContext context,
        Guid fileBusinessId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken);
}

public sealed record StagingUploadResult(string ObjectKey, long SizeBytes);
