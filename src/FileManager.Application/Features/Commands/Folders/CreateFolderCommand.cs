using FileManager.Contracts.Responses.Folders;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Folders;

public sealed record CreateFolderCommand(
    long ApplicationId,
    string Name,
    Guid? ParentFolderBusinessId) : IRequest<FolderResponse>;
