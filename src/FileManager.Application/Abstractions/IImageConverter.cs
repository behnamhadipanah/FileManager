namespace FileManager.Application.Abstractions;

public interface IImageConverter
{
    bool CanConvert(string contentType);

    Task<ConvertedMedia> ConvertToWebpAsync(Stream input, CancellationToken cancellationToken);
}

public sealed record ConvertedMedia(Stream Content, string ContentType, string FileExtension);
