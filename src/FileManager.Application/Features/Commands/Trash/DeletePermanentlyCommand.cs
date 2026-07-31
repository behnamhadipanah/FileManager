using FileManager.Contracts.Responses.Trash;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Trash;

public sealed record DeletePermanentlyCommand(
    long ApplicationId,
    long TrashItemId) : IRequest<DeletePermanentlyResponse>;
