using FileManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileManager.Infrastructure.BackgroundWorkers;

public sealed class FileUploadBackgroundWorker(
    FileUploadQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<FileUploadBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var uploadService = scope.ServiceProvider.GetRequiredService<FileUploadService>();
                await uploadService.ProcessQueuedUploadAsync(item, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex,
                    "Background upload failed for application {ApplicationId}, file {FileId}",
                    item.ApplicationId, item.FileId);
            }
        }
    }
}
