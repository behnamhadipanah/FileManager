using FileManager.Application.Abstractions;

namespace FileManager.Infrastructure.Storage;

public sealed class RustFsUploadStagingService(
    IFileStorageService fileStorageService,
    IApplicationBucketService bucketService) : IUploadStagingService
{
    public async Task<StagingUploadResult> SaveAsync(
        ApplicationStorageContext context,
        Guid fileBusinessId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        await bucketService.EnsureBucketsExistAsync(context.ApplicationName, context.FileType, cancellationToken);

        var objectKey = GetStagingObjectKey(fileBusinessId);
        var sizeBytes = content.CanSeek
            ? content.Length
            : await CopyAndMeasureAsync(context, objectKey, content, contentType, cancellationToken);

        if (content.CanSeek)
        {
            content.Position = 0;
            await fileStorageService.SaveFileAsync(context, objectKey, content, contentType, cancellationToken);
        }

        return new StagingUploadResult(objectKey, sizeBytes);
    }

    public Task<Stream> OpenReadAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken) =>
        fileStorageService.OpenFileAsync(context, objectKey, cancellationToken);

    public Task DeleteAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken) =>
        fileStorageService.DeleteFileAsync(context, objectKey, cancellationToken);

    private static string GetStagingObjectKey(Guid fileBusinessId) =>
        $"staging/{fileBusinessId:D}";

    private async Task<long> CopyAndMeasureAsync(
        ApplicationStorageContext context,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var sizeBytes = buffer.Length;
        buffer.Position = 0;
        await fileStorageService.SaveFileAsync(context, objectKey, buffer, contentType, cancellationToken);
        return sizeBytes;
    }
}
