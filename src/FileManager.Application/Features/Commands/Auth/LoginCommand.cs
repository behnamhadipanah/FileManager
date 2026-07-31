using FileManager.Contracts.Responses.Auth;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Auth;

public sealed record LoginCommand(string Email, string Password, bool RememberMe)
    : IRequest<AuthTokensResponse>;
