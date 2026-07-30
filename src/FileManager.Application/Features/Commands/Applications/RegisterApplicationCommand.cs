using FileManager.Contracts.Responses.Applications;
using Kootam.Cqrs.Abstractions.Commands;

namespace FileManager.Application.Features.Commands.Applications;

public sealed record RegisterApplicationCommand(
    string ApplicationName,
    long MinSizeUploadImage,
    long MaxSizeUploadImage,
    long MinSizeVideo,
    long MaxSizeVideo,
    long MinSizeDcoument,
    long MaxSizeDcoument) : IRequest<RegisterApplicationResponse>;
