using FileManager.Application.Abstractions;
using FileManager.Infrastructure.Storage.Abstractions;

namespace FileManager.Infrastructure.Storage;

public sealed class FileStorageService(
    IObjectStorage objectStorage,
    IApplicationBucketNaming bucketNaming,
    IApplicationBucketService bucketService) : IFileStorageService
{
    public async Task SaveFileAsync(
        ApplicationStorageContext context,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        await bucketService.EnsureBucketsExistAsync(context.ApplicationName, context.FileType, cancellationToken);
        await objectStorage.PutAsync(
            bucketNaming.GetFilesBucketName(context.ApplicationName, context.FileType),
            objectKey,
            content,
            contentType,
            cancellationToken);
    }

    public async Task SaveThumbnailAsync(
        ApplicationStorageContext context,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        await bucketService.EnsureBucketsExistAsync(context.ApplicationName, context.FileType, cancellationToken);
        await objectStorage.PutAsync(
            bucketNaming.GetThumbnailsBucketName(context.ApplicationName, context.FileType),
            objectKey,
            content,
            contentType,
            cancellationToken);
    }

    public Task<Stream> OpenFileAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken) =>
        objectStorage.GetAsync(
            bucketNaming.GetFilesBucketName(context.ApplicationName, context.FileType),
            objectKey,
            cancellationToken);

    public Task<Stream> OpenThumbnailAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken) =>
        objectStorage.GetAsync(
            bucketNaming.GetThumbnailsBucketName(context.ApplicationName, context.FileType),
            objectKey,
            cancellationToken);

    public Task DeleteFileAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken) =>
        objectStorage.DeleteAsync(
            bucketNaming.GetFilesBucketName(context.ApplicationName, context.FileType),
            objectKey,
            cancellationToken);

    public Task DeleteThumbnailAsync(
        ApplicationStorageContext context,
        string objectKey,
        CancellationToken cancellationToken) =>
        objectStorage.DeleteAsync(
            bucketNaming.GetThumbnailsBucketName(context.ApplicationName, context.FileType),
            objectKey,
            cancellationToken);
}
