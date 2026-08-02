using FileManager.Application.Abstractions;
using FFMpegCore;
using FFMpegCore.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace FileManager.Infrastructure.Storage;

public sealed class VideoConverter : IVideoConverter
{
    private static readonly WebpEncoder WebpEncoder = new() { Quality = 85 };

    public bool CanConvert(string contentType) =>
        contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);

    public async Task<ConvertedMedia> ConvertToWebmAsync(
        Stream input, string originalFileName, CancellationToken cancellationToken)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "filemanager", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var inputPath = Path.Combine(tempDir, $"input{Path.GetExtension(originalFileName)}");
        var outputPath = Path.Combine(tempDir, "output.webm");

        try
        {
            await using (var fileStream = File.Create(inputPath))
                await input.CopyToAsync(fileStream, cancellationToken);

            var success = await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(outputPath, overwrite: true, options => options
                    .WithVideoCodec(VideoCodec.LibVpx)
                    .WithAudioCodec(AudioCodec.LibVorbis))
                .CancellableThrough(cancellationToken)
                .ProcessAsynchronously();

            if (!success || !File.Exists(outputPath))
                throw new InvalidOperationException("Video conversion to WebM failed.");

            var output = new MemoryStream();
            await using (var converted = File.OpenRead(outputPath))
                await converted.CopyToAsync(output, cancellationToken);

            output.Position = 0;
            return new ConvertedMedia(output, "video/webm", ".webm");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    public async Task<ConvertedMedia> ExtractThumbnailAsync(
        Stream input,
        string originalFileName,
        int maxEdgeLength,
        CancellationToken cancellationToken)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "filemanager", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".mp4";

        var inputPath = Path.Combine(tempDir, $"input{extension}");
        var snapshotPath = Path.Combine(tempDir, "thumbnail.jpg");

        try
        {
            await using (var fileStream = File.Create(inputPath))
                await input.CopyToAsync(fileStream, cancellationToken);

            var success = await FFMpeg.SnapshotAsync(
                inputPath,
                snapshotPath,
                new System.Drawing.Size(maxEdgeLength, maxEdgeLength),
                captureTime: TimeSpan.FromSeconds(1),
                cancellationToken: cancellationToken);

            if (!success || !File.Exists(snapshotPath))
                throw new InvalidOperationException("Video thumbnail extraction failed.");

            await using var snapshotStream = File.OpenRead(snapshotPath);
            using var image = await Image.LoadAsync(snapshotStream, cancellationToken);

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
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
