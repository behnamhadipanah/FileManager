namespace FileManager.Application.Abstractions;

public interface IImageConverter
{
    bool CanConvert(string contentType);

    Task<ConvertedMedia> ConvertToWebpAsync(Stream input, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a resized WebP thumbnail from an image stream.
    /// </summary>
    Task<ConvertedMedia> CreateThumbnailAsync(
        Stream input,
        int maxEdgeLength,
        CancellationToken cancellationToken);
}

public sealed record ConvertedMedia(Stream Content, string ContentType, string FileExtension);
