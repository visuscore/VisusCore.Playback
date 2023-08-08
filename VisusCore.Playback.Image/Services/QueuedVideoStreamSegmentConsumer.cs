using System.Threading;
using System.Threading.Tasks;
using VisusCore.Consumer.Abstractions.Models;
using VisusCore.Consumer.Core.Services;

namespace VisusCore.Playback.Image.Services;

public class QueuedVideoStreamSegmentConsumer
    : QueuedVideoStreamSegmentConsumerBase<QueuedVideoStreamSegmentConsumerContext>
{
    public QueuedVideoStreamSegmentConsumer(QueuedVideoStreamSegmentConsumerContextAccessor contextAccessor)
        : base(contextAccessor)
    {
    }

    protected override Task SegmentQueuedAsync(
        QueuedVideoStreamSegmentConsumerContext context,
        IVideoStreamSegment segment,
        CancellationToken cancellationToken = default)
    {
        context.Sequence++;

        return Task.CompletedTask;
    }
}

public class QueuedVideoStreamSegmentConsumerContextAccessor
    : QueuedVideoStreamSegmentConsumerContextAccessorBase<QueuedVideoStreamSegmentConsumerContext>
{
    protected override QueuedVideoStreamSegmentConsumerContext CreateContext(string streamId) =>
        new(streamId, 1);
}

public sealed class QueuedVideoStreamSegmentConsumerContext : QueuedVideoStreamSegmentConsumerContextBase
{
    public long Sequence { get; set; }

    public QueuedVideoStreamSegmentConsumerContext(string streamId, int queueSize)
        : base(streamId, queueSize)
    {
    }
}
