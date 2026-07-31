using System.Threading.Channels;
using FileManager.Application.Abstractions;

namespace FileManager.Infrastructure.BackgroundWorkers;

public sealed class FileUploadQueue : IFileUploadQueue
{
    private readonly Channel<FileUploadWorkItem> _channel = Channel.CreateUnbounded<FileUploadWorkItem>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    internal ChannelReader<FileUploadWorkItem> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(FileUploadWorkItem item, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(item, cancellationToken);
}
