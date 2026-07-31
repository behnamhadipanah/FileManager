using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Aggregates.FileAgg;
using FileManager.Domain.Repositories;

namespace FileManager.Application.Mappers;

internal static class StorageFileMapper
{
    public static async Task<StorageFileResponse> ToResponseAsync(
        StorageFile file,
        IFolderRepository folderRepository,
        CancellationToken cancellationToken)
    {
        Guid? parentBusinessId = null;
        if (file.ParentFolderId is not null)
        {
            var parent = await folderRepository.GetAsync(file.ApplicationId, file.ParentFolderId.Value, cancellationToken);
            parentBusinessId = parent is not null ? (Guid)parent.BusinessId : null;
        }

        return ToResponse(file, parentBusinessId);
    }

    public static StorageFileResponse ToResponse(StorageFile file, Guid? parentFolderBusinessId) =>
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
            CreationTime = file.CreationTime
        };
}
