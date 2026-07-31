using FileManager.Application.Abstractions;
using FileManager.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace FileManager.Infrastructure.Storage;

public sealed class UploadStagingService(IOptions<UploadStagingOptions> options) : IUploadStagingService
{
    private readonly UploadStagingOptions _options = options.Value;

    public async Task<string> SaveAsync(Stream content, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.Path);
        var stagingPath = Path.Combine(_options.Path, $"{Guid.NewGuid():N}.upload");

        await using var fileStream = File.Create(stagingPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return stagingPath;
    }

    public Stream OpenRead(string stagingPath) => File.OpenRead(stagingPath);

    public Task DeleteAsync(string stagingPath, CancellationToken cancellationToken)
    {
        if (File.Exists(stagingPath))
            File.Delete(stagingPath);

        return Task.CompletedTask;
    }
}
