using FileManager.Contracts.Responses.Folders;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Folders;

public sealed record DeleteFolderPermanentlyCommand(
    long ApplicationId,
    Guid FolderBusinessId) : IRequest<DeleteFolderPermanentlyResponse>;
