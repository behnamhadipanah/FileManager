using FileManager.Contracts.Responses.Auth;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Auth;

public sealed record RefreshTokenCommand : IRequest<AuthTokensResponse>;
