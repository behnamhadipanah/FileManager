using FileManager.Application.Abstractions;
using FileManager.Domain.ValueObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace FileManager.Infrastructure.Storage;

public sealed class ImageConverter : IImageConverter
{
    public bool CanConvert(string contentType)
    {
        var mimeType = MimeType.FromString(contentType);
        return mimeType.IsImage() && !mimeType.IsSvg();
    }

    public async Task<ConvertedMedia> ConvertToWebpAsync(Stream input, CancellationToken cancellationToken)
    {
        using var image = await Image.LoadAsync(input, cancellationToken);
        var output = new MemoryStream();
        await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = 85 }, cancellationToken);
        output.Position = 0;
        return new ConvertedMedia(output, "image/webp", ".webp");
    }
}
