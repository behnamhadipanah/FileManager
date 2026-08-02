using FileManager.Contracts.Responses.Applications;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Applications;

public sealed record RegenerateApplicationTokenCommand(long ApplicationId)
    : IRequest<RegenerateApplicationTokenResponse>;
