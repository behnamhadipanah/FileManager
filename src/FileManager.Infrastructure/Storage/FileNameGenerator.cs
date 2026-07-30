using FileManager.Domain.Services;

namespace FileManager.Infrastructure.Storage;

public sealed class FileNameGenerator : IFileNameGenerator
{
    public string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid():N}{extension}";
    }
}
