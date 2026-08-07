using FileManager.Application.Abstractions;
using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Files;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Queries.Files;

public sealed record GetFileQuery(
    long ApplicationId,
    Guid FileBusinessId,
    bool? IsDeleted = null) : IQuery<StorageFileResponse>;

public sealed class GetFileQueryHandler(
    IApplicationRepository applicationRepository,
    IStorageFileRepository storageFileRepository,
    IFolderRepository folderRepository,
    IFileStorageService fileStorageService)
    : IQueryHandler<GetFileQuery, StorageFileResponse>
{
    public async Task<Result<StorageFileResponse>> Handle(
        GetFileQuery query, CancellationToken cancellationToken = default)
    {
        var file = await storageFileRepository.GetByBusinessIdAsync(
            query.ApplicationId, BusinessId.FromGuid(query.FileBusinessId), cancellationToken);

        if (file is null)
            return Result<StorageFileResponse>.Failure(ResultStatus.NotFound, DomainMessages.FileNotFound);

        if (query.IsDeleted is not null && file.IsDeleted != query.IsDeleted)
            return Result<StorageFileResponse>.Failure(ResultStatus.NotFound, DomainMessages.FileNotFound);

        var application = await applicationRepository.GetAsync(query.ApplicationId, cancellationToken);
        if (application is null)
            return Result<StorageFileResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        return Result<StorageFileResponse>.Success(
            await StorageFileMapper.ToResponseAsync(
                file,
                application.ApplicationName,
                folderRepository,
                fileStorageService,
                cancellationToken));
    }
}
