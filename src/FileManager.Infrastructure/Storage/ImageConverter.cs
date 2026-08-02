using FileManager.Application.Abstractions;
using FileManager.Domain.ValueObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace FileManager.Infrastructure.Storage;

public sealed class ImageConverter : IImageConverter
{
    private static readonly WebpEncoder WebpEncoder = new() { Quality = 85 };

    public bool CanConvert(string contentType)
    {
        var mimeType = MimeType.FromString(contentType);
        return mimeType.IsImage() && !mimeType.IsSvg();
    }

    public async Task<ConvertedMedia> ConvertToWebpAsync(Stream input, CancellationToken cancellationToken)
    {
        using var image = await Image.LoadAsync(input, cancellationToken);
        var output = new MemoryStream();
        await image.SaveAsWebpAsync(output, WebpEncoder, cancellationToken);
        output.Position = 0;
        return new ConvertedMedia(output, "image/webp", ".webp");
    }

    public async Task<ConvertedMedia> CreateThumbnailAsync(
        Stream input,
        int maxEdgeLength,
        CancellationToken cancellationToken)
    {
        using var image = await Image.LoadAsync(input, cancellationToken);

        if (image.Width > maxEdgeLength || image.Height > maxEdgeLength)
        {
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxEdgeLength, maxEdgeLength)
            }));
        }

        var output = new MemoryStream();
        await image.SaveAsWebpAsync(output, WebpEncoder, cancellationToken);
        output.Position = 0;
        return new ConvertedMedia(output, "image/webp", ".webp");
    }
}
