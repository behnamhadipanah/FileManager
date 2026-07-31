using FileManager.Contracts.Responses.Trash;
using FileManager.Domain.Enumerations;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Trash;

public sealed record MoveToTrashCommand(
    long ApplicationId,
    TrashItemType ItemType,
    long ItemId) : IRequest<MoveToTrashResponse>;
