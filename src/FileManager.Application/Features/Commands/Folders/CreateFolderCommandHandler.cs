using FileManager.Application.Features.Commands.Folders;
using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Aggregates.FolderAgg;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using FileManager.Domain.ValueObjects;
using Kootam.Cqrs.Abstractions.Commands;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Commands.Folders;

public sealed class CreateFolderCommandHandler(IFolderRepository folderRepository)
    : IRequestHandler<CreateFolderCommand, FolderResponse>
{
    public async Task<Result<FolderResponse>> Handle(
        CreateFolderCommand command, CancellationToken cancellationToken = default)
    {
        var (parent, errorStatus, errorMessage) = await FolderParentResolver.ResolveAsync(
            folderRepository, command.ApplicationId, command.ParentFolderBusinessId, cancellationToken);

        if (errorStatus is not null)
            return Result<FolderResponse>.Failure(errorStatus.Value, errorMessage!);

        var name = FileName.FromString(command.Name.Trim());

        if (await folderRepository.ExistsByNameAsync(command.ApplicationId, parent!.Id, name.Value, cancellationToken))
            return Result<FolderResponse>.Failure(ResultStatus.Conflict, DomainMessages.FolderNameExists);

        var now = DateTime.UtcNow;
        var folder = Folder.Create(command.ApplicationId, name, parent!.Id, now);
        await folderRepository.InsertAsync(folder, cancellationToken);

        return Result<FolderResponse>.Success(
            FolderMapper.ToResponse(folder, parent!.BusinessId));
    }
}
