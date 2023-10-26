using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using VisusCore.Consumer.Abstractions.Models;
using VisusCore.Consumer.Core.Services;
using VisusCore.Playback.Hls.Models;

namespace VisusCore.Playback.Hls.Services;

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
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Sequence++;

        return Task.CompletedTask;
    }
}

public class QueuedVideoStreamSegmentConsumerContextAccessor
    : QueuedVideoStreamSegmentConsumerContextAccessorBase<QueuedVideoStreamSegmentConsumerContext>
{
    private readonly IOptions<HlsOptions> _hlsOptions;

    public QueuedVideoStreamSegmentConsumerContextAccessor(IOptions<HlsOptions> hlsOptions) =>
        _hlsOptions = hlsOptions;

    protected override QueuedVideoStreamSegmentConsumerContext CreateContext(string streamId) =>
        new(streamId, _hlsOptions.Value.LiveCacheRetention + _hlsOptions.Value.LiveMinSegmentsCount);
}

public sealed class QueuedVideoStreamSegmentConsumerContext : QueuedVideoStreamSegmentConsumerContextBase
{
    public long Sequence { get; set; }

    public QueuedVideoStreamSegmentConsumerContext(string streamId, int queueSize)
        : base(streamId, queueSize)
    {
    }
}
