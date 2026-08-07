using FileManager.Application.Abstractions;
using FileManager.Application.Mappers;
using FileManager.Contracts.Responses.Folders;
using FileManager.Domain.Messages;
using FileManager.Domain.Repositories;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Cqrs.Abstractions.Queries;
using Kootam.Framework.Domain.ValueObjects;

namespace FileManager.Application.Features.Queries.Folders;

public sealed class GetFolderContentsQueryHandler(
    IApplicationRepository applicationRepository,
    IFolderRepository folderRepository,
    IStorageFileRepository storageFileRepository,
    IFileStorageService fileStorageService)
    : IQueryHandler<GetFolderContentsQuery, FolderContentsResponse>
{
    public async Task<Result<FolderContentsResponse>> Handle(
        GetFolderContentsQuery query, CancellationToken cancellationToken = default)
    {
        if (!await applicationRepository.ExistsAsync(query.ApplicationId, cancellationToken))
            return Result<FolderContentsResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        var application = await applicationRepository.GetAsync(query.ApplicationId, cancellationToken);
        if (application is null)
            return Result<FolderContentsResponse>.Failure(ResultStatus.NotFound, DomainMessages.ApplicationNotFound);

        var currentFolder = query.FolderBusinessId is null
            ? await folderRepository.GetRootAsync(query.ApplicationId, cancellationToken)
            : await folderRepository.GetByBusinessIdAsync(
                query.ApplicationId,
                BusinessId.FromGuid(query.FolderBusinessId.Value),
                cancellationToken);

        if (currentFolder is null)
            return Result<FolderContentsResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        if (query.IsDeleted is not null && currentFolder.IsDeleted != query.IsDeleted)
            return Result<FolderContentsResponse>.Failure(ResultStatus.NotFound, DomainMessages.FolderNotFound);

        var childFolders = await folderRepository.GetByParentFolderIdAsync(
            query.ApplicationId, currentFolder.Id, query.IsDeleted, cancellationToken);

        var childFiles = await storageFileRepository.GetByParentFolderIdAsync(
            query.ApplicationId, currentFolder.Id, query.IsDeleted, cancellationToken);

        var folderResponses = new List<FolderResponse>(childFolders.Count);
        foreach (var folder in childFolders)
        {
            folderResponses.Add(await FolderMapper.ToResponseAsync(folder, folderRepository, cancellationToken));
        }

        var currentFolderBusinessId = (Guid)currentFolder.BusinessId;
        var fileResponses = childFiles
            .Select(file => StorageFileMapper.ToResponse(
                file,
                currentFolderBusinessId,
                application.ApplicationName,
                fileStorageService))
            .ToList();

        return Result<FolderContentsResponse>.Success(new FolderContentsResponse
        {
            CurrentFolder = await FolderMapper.ToResponseAsync(currentFolder, folderRepository, cancellationToken),
            Folders = folderResponses,
            Files = fileResponses
        });
    }
}
