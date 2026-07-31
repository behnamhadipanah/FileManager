namespace FileManager.Application.Abstractions;

public interface IVideoConverter
{
    bool CanConvert(string contentType);

    Task<ConvertedMedia> ConvertToWebmAsync(Stream input, string originalFileName, CancellationToken cancellationToken);
}
