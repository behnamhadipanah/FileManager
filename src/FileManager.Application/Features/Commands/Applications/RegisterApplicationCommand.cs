using FileManager.Contracts.Responses.Applications;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Applications;

public sealed record RegisterApplicationCommand(
    string ApplicationName,
    long MinImageSizeKilobytes,
    long MaxImageSizeKilobytes,
    long MinVideoSizeKilobytes,
    long MaxVideoSizeKilobytes,
    long MinDocumentSizeKilobytes,
    long MaxDocumentSizeKilobytes) : IRequest<RegisterApplicationResponse>;
