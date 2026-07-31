using FileManager.Contracts.Responses.Folders;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Folders;

public sealed record UpdateFolderCommand(
    long ApplicationId,
    Guid FolderBusinessId,
    Guid? ParentFolderBusinessId) : IRequest<FolderResponse>;
