using FileManager.Domain.Aggregates.FolderAgg;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using FileManager.Domain.ValueObjects;
using Kootam.Cqrs.Abstractions.Enums;

namespace FileManager.Application.Features.Commands.Folders;

internal static class FolderHierarchyRestorer
{
    internal static async Task<(long? ParentFolderId, ResultStatus? ErrorStatus, string? ErrorMessage)> EnsureParentFolderChainAsync(
        IFolderRepository folderRepository,
        ITrashRepository trashRepository,
        long applicationId,
        long? parentFolderId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (parentFolderId is null)
            return (null, null, null);

        var folder = await folderRepository.GetAsync(applicationId, parentFolderId.Value, cancellationToken);
        if (folder is null)
            return await RecreateMissingFolderAsync(folderRepository, applicationId, parentFolderId.Value, now, cancellationToken);

        if (!folder.IsDeleted)
            return (folder.Id, null, null);

        var (ensuredGrandparentId, errorStatus, errorMessage) = await EnsureParentFolderChainAsync(
            folderRepository, trashRepository, applicationId, folder.ParentFolderId, now, cancellationToken);
        if (errorStatus is not null)
            return (null, errorStatus, errorMessage);

        if (folder.ParentFolderId != ensuredGrandparentId)
            folder.Move(ensuredGrandparentId, now);

        var trashItem = await trashRepository.GetByItemAsync(
            applicationId, TrashItemType.Folder, folder.Id, cancellationToken);
        if (trashItem is not null && !trashItem.IsRestored && !trashItem.IsPurged)
        {
            trashItem.Restore(now);
            await trashRepository.UpdateAsync(trashItem, cancellationToken);
        }

        folder.Restore();
        await folderRepository.UpdateAsync(folder, cancellationToken);

        return (folder.Id, null, null);
    }

    private static async Task<(long? ParentFolderId, ResultStatus? ErrorStatus, string? ErrorMessage)> RecreateMissingFolderAsync(
        IFolderRepository folderRepository,
        long applicationId,
        long missingFolderId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var root = await folderRepository.GetRootAsync(applicationId, cancellationToken);
        if (root is null)
            return (null, ResultStatus.NotFound, DomainMessages.FolderNotFound);

        var folderName = FileName.FromString($"Restored-{missingFolderId}");
        var suffix = 0;
        while (await folderRepository.ExistsByNameAsync(applicationId, root.Id, folderName.Value, cancellationToken))
            folderName = FileName.FromString($"Restored-{missingFolderId}-{++suffix}");

        var recreated = Folder.Create(applicationId, folderName, root.Id, now);
        await folderRepository.InsertAsync(recreated, cancellationToken);

        return (recreated.Id, null, null);
    }
}
