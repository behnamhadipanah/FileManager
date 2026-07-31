namespace FileManager.Application.Abstractions;

public sealed record FileUploadWorkItem(
    long ApplicationId,
    long FileId,
    string StagingObjectKey,
    string OriginalFileName,
    string ContentType);

public interface IFileUploadQueue
{
    ValueTask EnqueueAsync(FileUploadWorkItem item, CancellationToken cancellationToken = default);
}
