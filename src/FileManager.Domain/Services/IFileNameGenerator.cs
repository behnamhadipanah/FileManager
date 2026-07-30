namespace FileManager.Domain.Services;

public interface IFileNameGenerator
{
    string GenerateUniqueFileName(string originalFileName);
}
