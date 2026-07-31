using FileManager.Application.Abstractions;
using FFMpegCore;
using FFMpegCore.Enums;

namespace FileManager.Infrastructure.Storage;

public sealed class VideoConverter : IVideoConverter
{
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
}
