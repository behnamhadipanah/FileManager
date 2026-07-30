using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using FileManager.Infrastructure.Storage.Abstractions;

namespace FileManager.Infrastructure.Storage.RustFs;

/// <summary>
/// RustFS speaks the S3 protocol, so it is accessed through the standard AWS SDK
/// pointed at RustFS's endpoint (see RustFsOptions / ServiceCollectionExtensions).
/// </summary>
public sealed class RustFsObjectStorage(IAmazonS3 s3Client) : IObjectStorage
{
    public async Task PutAsync(string bucket, string objectKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(bucket, cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<Stream> GetAsync(string bucket, string objectKey, CancellationToken cancellationToken)
    {
        var response = await s3Client.GetObjectAsync(bucket, objectKey, cancellationToken);
        return response.ResponseStream;
    }

    public Task DeleteAsync(string bucket, string objectKey, CancellationToken cancellationToken) =>
        s3Client.DeleteObjectAsync(bucket, objectKey, cancellationToken);

    public async Task<bool> ExistsAsync(string bucket, string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            await s3Client.GetObjectMetadataAsync(bucket, objectKey, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken)
    {
        var exists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucket);
        if (!exists)
            await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucket }, cancellationToken);
    }
}
