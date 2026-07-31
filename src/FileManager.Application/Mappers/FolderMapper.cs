using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Aggregates.FolderAgg;
using FileManager.Domain.Repositories;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Mappers;

internal static class FolderMapper
{
    public static async Task<FolderResponse> ToResponseAsync(
        Folder folder,
        IFolderRepository folderRepository,
        CancellationToken cancellationToken)
    {
        Guid? parentBusinessId = null;
        if (folder.ParentFolderId is not null)
        {
            var parent = await folderRepository.GetAsync(folder.ApplicationId, folder.ParentFolderId.Value, cancellationToken);
            parentBusinessId = parent is not null ? (Guid)parent.BusinessId : null;
        }

        return new FolderResponse
        {
            BusinessId = (Guid)folder.BusinessId,
            Name = folder.Name.Value,
            ParentFolderBusinessId = parentBusinessId,
            IsDeleted = folder.IsDeleted,
            DeletionTime = folder.DeletionTime,
            CreationTime = folder.CreationTime
        };
    }

    public static FolderResponse ToResponse(Folder folder, BusinessId? parentFolderBusinessId) =>
        new()
        {
            BusinessId = (Guid)folder.BusinessId,
            Name = folder.Name.Value,
            ParentFolderBusinessId = parentFolderBusinessId is not null ? (Guid)parentFolderBusinessId : null,
            IsDeleted = folder.IsDeleted,
            DeletionTime = folder.DeletionTime,
            CreationTime = folder.CreationTime
        };
}
