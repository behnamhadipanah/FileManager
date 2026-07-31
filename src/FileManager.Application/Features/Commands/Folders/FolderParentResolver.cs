using FileManager.Domain.Aggregates.FolderAgg;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Folders;

internal static class FolderParentResolver
{
    internal static async Task<(Folder? Folder, ResultStatus? ErrorStatus, string? ErrorMessage)> ResolveAsync(
        IFolderRepository folderRepository,
        long applicationId,
        Guid? parentFolderBusinessId,
        CancellationToken cancellationToken)
    {
        if (parentFolderBusinessId is null)
        {
            var root = await folderRepository.GetRootAsync(applicationId, cancellationToken);
            return root is null
                ? (null, ResultStatus.NotFound, DomainMessages.FolderNotFound)
                : (root, null, null);
        }

        var parent = await folderRepository.GetByBusinessIdAsync(
            applicationId, BusinessId.FromGuid(parentFolderBusinessId.Value), cancellationToken);

        if (parent is null)
            return (null, ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (parent.IsDeleted)
            return (null, ResultStatus.ValidationError, DomainMessages.InvalidParentFolder);

        return (parent, null, null);
    }
}
