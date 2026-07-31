using FileManager.Application.Abstractions;
using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Aggregates.ApplicationAgg;
using FileManager.Domain.Aggregates.FileAgg;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Policies;
using FileManager.Domain.Repositories;
using FileManager.Domain.Services;
using FileManager.Domain.ValueObjects;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Services;

public sealed class FileUploadService(
    IApplicationRepository applicationRepository,
    IStorageFileRepository storageFileRepository,
    IFolderRepository folderRepository,
    IFileStorageService fileStorageService,
    IApplicationBucketService bucketService,
    IHashCalculator hashCalculator,
    IStoragePathGenerator storagePathGenerator,
    IImageConverter imageConverter,
    IVideoConverter videoConverter,
    IUploadStagingService uploadStagingService,
    IFileUploadQueue uploadQueue)
{
    public async Task<Result<StorageFileResponse>> EnqueueUploadAsync(
        long applicationId,
        long? parentFolderId,
        string originalFileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetAsync(applicationId, cancellationToken);
        if (application is null)
            return Result<StorageFileResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        if (!application.IsActive)
            return Result<StorageFileResponse>.Failure(ResultStatus.ValidationError, DomainMessages.ApplicationInactive);

        var mimeType = MimeType.FromString(string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType);
        var fileType = UploadValidationPolicy.ResolveFileType(mimeType);
        var displayName = FileName.FromString(originalFileName);
        var storageContext = new ApplicationStorageContext(application.ApplicationName, fileType);

        if (await storageFileRepository.ExistsByNameAsync(applicationId, parentFolderId, displayName.Value, cancellationToken))
            return Result<StorageFileResponse>.Failure(ResultStatus.Conflict, DomainMessages.FileNameExists);

        await bucketService.EnsureBucketsExistAsync(application.ApplicationName, fileType, cancellationToken);

        await using var contentBuffer = new MemoryStream();
        await content.CopyToAsync(contentBuffer, cancellationToken);
        contentBuffer.Position = 0;
        var stagingSize = FileSize.FromBytes(contentBuffer.Length);

        try
        {
            UploadValidationPolicy.EnsureWithinLimits(application.UploadLimits, fileType, stagingSize);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result<StorageFileResponse>.Failure(ResultStatus.ValidationError, ex.Message);
        }

        var now = DateTime.UtcNow;
        var file = StorageFile.CreatePendingUpload(
            applicationId,
            parentFolderId,
            displayName,
            mimeType,
            stagingSize,
            StorageProvider.RustFs,
            fileType,
            now);

        var fileBusinessId = (Guid)file.BusinessId;
        var staging = await uploadStagingService.SaveAsync(
            storageContext, fileBusinessId, contentBuffer, contentType, cancellationToken);

        await storageFileRepository.InsertAsync(file, cancellationToken);

        await uploadQueue.EnqueueAsync(new FileUploadWorkItem(
            applicationId,
            file.Id,
            staging.ObjectKey,
            originalFileName,
            contentType), cancellationToken);

        return Result<StorageFileResponse>.Success(
            await StorageFileMapper.ToResponseAsync(file, folderRepository, cancellationToken));
    }

    public async Task ProcessQueuedUploadAsync(FileUploadWorkItem item, CancellationToken cancellationToken)
    {
        var file = await storageFileRepository.GetAsync(item.ApplicationId, item.FileId, cancellationToken);
        if (file is null || file.UploadStatus is not UploadStatus.Pending)
            return;

        var application = await applicationRepository.GetAsync(item.ApplicationId, cancellationToken)
            ?? throw new InvalidOperationException(DomainMessages.ApplicationNotFound);

        var storageContext = new ApplicationStorageContext(application.ApplicationName, file.FileType);
        var fileBusinessId = (Guid)file.BusinessId;

        var now = DateTime.UtcNow;
        file.StartUploadProcessing(now);
        await storageFileRepository.UpdateAsync(file, cancellationToken);

        try
        {
            await using var stagingStream = await uploadStagingService.OpenReadAsync(
                storageContext, item.StagingObjectKey, cancellationToken);

            var processed = await ProcessContentAsync(
                application,
                fileBusinessId,
                item.OriginalFileName,
                item.ContentType,
                stagingStream,
                cancellationToken);

            try
            {
                await fileStorageService.SaveFileAsync(
                    storageContext,
                    processed.ObjectKey.Value,
                    processed.Content,
                    processed.MimeType.Value,
                    cancellationToken);

                file.CompleteUpload(
                    FileName.FromString(processed.FileName),
                    processed.MimeType,
                    processed.Size,
                    processed.ContentHash,
                    processed.ObjectKey,
                    processed.FileType,
                    processed.Converted,
                    now);

                await storageFileRepository.UpdateAsync(file, cancellationToken);
            }
            finally
            {
                await processed.Content.DisposeAsync();
            }
        }
        catch
        {
            file.FailUpload(now);
            await storageFileRepository.UpdateAsync(file, cancellationToken);
            throw;
        }
        finally
        {
            await uploadStagingService.DeleteAsync(storageContext, item.StagingObjectKey, cancellationToken);
        }
    }

    private async Task<ProcessedUploadContent> ProcessContentAsync(
        RegisteredApplication application,
        Guid fileBusinessId,
        string originalFileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken)
    {
        var mimeType = MimeType.FromString(string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType);
        var fileType = UploadValidationPolicy.ResolveFileType(mimeType);

        await using var workingStream = new MemoryStream();
        await content.CopyToAsync(workingStream, cancellationToken);
        workingStream.Position = 0;

        var converted = false;
        var finalFileName = originalFileName;
        var finalMimeType = mimeType;
        Stream uploadStream = workingStream;
        MemoryStream? convertedStream = null;

        try
        {
            if (imageConverter.CanConvert(mimeType.Value))
            {
                var convertedMedia = await imageConverter.ConvertToWebpAsync(workingStream, cancellationToken);
                convertedStream = convertedMedia.Content as MemoryStream
                    ?? await CopyToMemoryStreamAsync(convertedMedia.Content, cancellationToken);
                uploadStream = convertedStream;
                finalMimeType = MimeType.FromString(convertedMedia.ContentType);
                finalFileName = Path.ChangeExtension(originalFileName, convertedMedia.FileExtension)
                    ?? $"{originalFileName}.webp";
                converted = true;
            }
            else if (videoConverter.CanConvert(mimeType.Value))
            {
                workingStream.Position = 0;
                var convertedMedia = await videoConverter.ConvertToWebmAsync(workingStream, originalFileName, cancellationToken);
                convertedStream = convertedMedia.Content as MemoryStream
                    ?? await CopyToMemoryStreamAsync(convertedMedia.Content, cancellationToken);
                uploadStream = convertedStream;
                finalMimeType = MimeType.FromString(convertedMedia.ContentType);
                finalFileName = Path.ChangeExtension(originalFileName, convertedMedia.FileExtension)
                    ?? $"{originalFileName}.webm";
                converted = true;
            }

            var fileSize = FileSize.FromBytes(uploadStream.Length);
            UploadValidationPolicy.EnsureWithinLimits(application.UploadLimits, fileType, fileSize);

            uploadStream.Position = 0;
            var contentHash = await hashCalculator.ComputeAsync(uploadStream, cancellationToken);
            uploadStream.Position = 0;

            var objectKey = storagePathGenerator.GenerateObjectKey(fileBusinessId);

            var outputStream = new MemoryStream();
            await uploadStream.CopyToAsync(outputStream, cancellationToken);
            outputStream.Position = 0;

            return new ProcessedUploadContent(
                outputStream,
                finalFileName,
                finalMimeType,
                fileSize,
                contentHash,
                objectKey,
                fileType,
                converted);
        }
        finally
        {
            if (convertedStream is not null && !ReferenceEquals(convertedStream, workingStream))
                await convertedStream.DisposeAsync();
        }
    }

    private static async Task<MemoryStream> CopyToMemoryStreamAsync(Stream source, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();
        await source.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    private sealed record ProcessedUploadContent(
        MemoryStream Content,
        string FileName,
        MimeType MimeType,
        FileSize Size,
        ContentHash ContentHash,
        StorageObjectKey ObjectKey,
        StorageFileType FileType,
        bool Converted);
}
