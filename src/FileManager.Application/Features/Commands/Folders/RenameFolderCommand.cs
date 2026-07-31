using FileManager.Contracts.Responses.Folders;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Folders;

public sealed record RenameFolderCommand(
    long ApplicationId,
    Guid FolderBusinessId,
    string Name) : IRequest<FolderResponse>;
