namespace FileManager.Application.Abstractions;

public interface IVideoConverter
{
    bool CanConvert(string contentType);

    Task<ConvertedMedia> ConvertToWebmAsync(Stream input, string originalFileName, CancellationToken cancellationToken);

    /// <summary>
    /// Extracts a single frame from a video and returns it as a WebP thumbnail.
    /// </summary>
    Task<ConvertedMedia> ExtractThumbnailAsync(
        Stream input,
        string originalFileName,
        int maxEdgeLength,
        CancellationToken cancellationToken);
}
