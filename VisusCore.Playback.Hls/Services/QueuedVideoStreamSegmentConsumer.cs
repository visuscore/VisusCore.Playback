using System.Threading;
using System.Threading.Tasks;
using VisusCore.Consumer.Abstractions.Models;
using VisusCore.Consumer.Abstractions.Services;

namespace VisusCore.Playback.Hls.Services;

public class QueuedVideoStreamSegmentConsumer : IVideoStreamSegmentConsumer
{
    private readonly QueuedVideoStreamSegmentConsumerContextAccessor _contextAccessor;

    public QueuedVideoStreamSegmentConsumer(QueuedVideoStreamSegmentConsumerContextAccessor contextAccessor) =>
        _contextAccessor = contextAccessor;

    public Task ConsumeAsync(IVideoStreamSegment segment, CancellationToken cancellationToken = default) =>
        _contextAccessor.InvokeLockedAsync(
            segment.StreamId,
            context =>
            {
                context.Cache.Enqueue(segment);
                context.Sequence++;

                return Task.CompletedTask;
            },
            cancellationToken);
}
