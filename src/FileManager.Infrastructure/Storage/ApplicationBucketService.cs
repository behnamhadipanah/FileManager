using FileManager.Application.Abstractions;
using FileManager.Domain.Enumerations;
using FileManager.Infrastructure.Storage.Abstractions;

namespace FileManager.Infrastructure.Storage;

public interface IApplicationBucketNaming
{
    string GetFilesBucketName(string applicationName, StorageFileType fileType);
    string GetThumbnailsBucketName(string applicationName, StorageFileType fileType);
}

public sealed class ApplicationBucketNaming : IApplicationBucketNaming
{
    private const int MaxBucketNameLength = 63;

    public string GetFilesBucketName(string applicationName, StorageFileType fileType) =>
        BuildBucketName(SanitizeApplicationName(applicationName), GetTypeSuffix(fileType));

    public string GetThumbnailsBucketName(string applicationName, StorageFileType fileType) =>
        BuildBucketName(SanitizeApplicationName(applicationName), GetTypeSuffix(fileType), "thumbnail");

    internal static string SanitizeApplicationName(string applicationName)
    {
        var sanitized = new string(applicationName
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());

        while (sanitized.Contains("--", StringComparison.Ordinal))
            sanitized = sanitized.Replace("--", "-", StringComparison.Ordinal);

        sanitized = sanitized.Trim('-');
        return string.IsNullOrWhiteSpace(sanitized) ? "application" : sanitized;
    }

    internal static string GetTypeSuffix(StorageFileType fileType) => fileType switch
    {
        StorageFileType.Image => "images",
        StorageFileType.Video => "videos",
        StorageFileType.Document => "documents",
        StorageFileType.Audio => "audio",
        StorageFileType.Archive => "archives",
        _ => "files"
    };

    private static string BuildBucketName(string applicationName, params string[] suffixParts)
    {
        var bucketName = string.Join('-', new[] { applicationName }.Concat(suffixParts));

        if (bucketName.Length > MaxBucketNameLength)
            bucketName = bucketName[..MaxBucketNameLength].TrimEnd('-');

        if (bucketName.Length < 3)
            bucketName = bucketName.PadRight(3, '0');

        return bucketName;
    }
}

public sealed class ApplicationBucketService(
    IObjectStorage objectStorage,
    IApplicationBucketNaming bucketNaming) : IApplicationBucketService
{
    public async Task EnsureApplicationBucketsAsync(string applicationName, CancellationToken cancellationToken = default)
    {
        foreach (StorageFileType fileType in Enum.GetValues<StorageFileType>())
        {
            if (fileType is StorageFileType.Unknown)
                continue;

            await EnsureBucketsExistAsync(applicationName, fileType, cancellationToken);
        }
    }

    public async Task EnsureBucketsExistAsync(
        string applicationName, StorageFileType fileType, CancellationToken cancellationToken = default)
    {
        await objectStorage.EnsureBucketExistsAsync(
            bucketNaming.GetFilesBucketName(applicationName, fileType), cancellationToken);

        if (fileType is StorageFileType.Image or StorageFileType.Video)
        {
            await objectStorage.EnsureBucketExistsAsync(
                bucketNaming.GetThumbnailsBucketName(applicationName, fileType), cancellationToken);
        }
    }
}
