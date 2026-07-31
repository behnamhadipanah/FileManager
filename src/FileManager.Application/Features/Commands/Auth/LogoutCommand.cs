using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Auth;

public sealed record LogoutCommand : IRequest<bool>;
