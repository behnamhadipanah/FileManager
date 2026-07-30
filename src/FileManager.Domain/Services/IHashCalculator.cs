using FileManager.Domain.ValueObjects;

namespace FileManager.Domain.Services;

public interface IHashCalculator
{
    Task<ContentHash> ComputeAsync(Stream content, CancellationToken cancellationToken);
}
