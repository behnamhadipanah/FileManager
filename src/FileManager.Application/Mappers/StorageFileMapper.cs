using FileManager.Application.Abstractions;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Aggregates.FileAgg;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Repositories;

namespace FileManager.Application.Mappers;

internal static class StorageFileMapper
{
    public static async Task<StorageFileResponse> ToResponseAsync(
        StorageFile file,
        string applicationName,
        IFolderRepository folderRepository,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        Guid? parentBusinessId = null;
        if (file.ParentFolderId is not null)
        {
            var parent = await folderRepository.GetAsync(file.ApplicationId, file.ParentFolderId.Value, cancellationToken);
            parentBusinessId = parent is not null ? (Guid)parent.BusinessId : null;
        }

        return ToResponse(file, parentBusinessId, applicationName, fileStorageService);
    }

    public static StorageFileResponse ToResponse(
        StorageFile file,
        Guid? parentFolderBusinessId,
        string applicationName,
        IFileStorageService fileStorageService) =>
        new()
        {
            BusinessId = (Guid)file.BusinessId,
            Name = file.Name.Value,
            MimeType = file.MimeType.Value,
            SizeBytes = file.Size.Bytes,
            ContentHash = file.ContentHash.Value,
            ParentFolderBusinessId = parentFolderBusinessId,
            FileType = (int)file.FileType,
            ConversionStatus = (int)file.ConversionStatus,
            UploadStatus = (int)file.UploadStatus,
            ThumbnailStatus = (int)file.ThumbnailStatus,
            IsDeleted = file.IsDeleted,
            DeletionTime = file.DeletionTime,
            CreationTime = file.CreationTime,
            PublicUrl = ResolvePublicUrl(file, applicationName, fileStorageService)
        };

    private static string? ResolvePublicUrl(
        StorageFile file,
        string applicationName,
        IFileStorageService fileStorageService)
    {
        if (file.UploadStatus is not UploadStatus.Completed)
            return null;

        var storageContext = new ApplicationStorageContext(applicationName, file.FileType);
        return fileStorageService.GetPublicFileUrl(storageContext, file.ObjectKey.Value);
    }
}
