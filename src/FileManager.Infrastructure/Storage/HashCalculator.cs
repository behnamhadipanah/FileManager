using System.Security.Cryptography;
using FileManager.Domain.Services;
using FileManager.Domain.ValueObjects;

namespace FileManager.Infrastructure.Storage;

public sealed class HashCalculator : IHashCalculator
{
    public async Task<ContentHash> ComputeAsync(Stream content, CancellationToken cancellationToken)
    {
        var hashBytes = await SHA256.HashDataAsync(content, cancellationToken);
        return ContentHash.FromString(Convert.ToHexStringLower(hashBytes));
    }
}
