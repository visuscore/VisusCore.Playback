using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VisusCore.Consumer.Abstractions.Models;
using VisusCore.Playback.Hls.Models;

namespace VisusCore.Playback.Hls.Services;

public class QueuedVideoStreamSegmentConsumerContextAccessor
{
    private readonly IOptions<HlsOptions> _hlsOptions;
    private readonly ConcurrentDictionary<string, QueuedVideoStreamSegmentConsumerContext> _contexts = new();

    public QueuedVideoStreamSegmentConsumerContextAccessor(IOptions<HlsOptions> hlsOptions) =>
        _hlsOptions = hlsOptions;

    public bool IsExists(string streamId) => _contexts.ContainsKey(streamId);

    public async Task InvokeLockedAsync(
        string streamId,
        Func<QueuedVideoStreamSegmentConsumerContext, Task> actionAsync,
        CancellationToken cancellationToken = default)
    {
        var context = GetOrAddContext(streamId);

        await context.ConsumeLock.WaitAsync(cancellationToken);

        try
        {
            await actionAsync(context);
        }
        finally
        {
            context.ConsumeLock.Release();
        }
    }

    public async Task<TResult> InvokeLockedAsync<TResult>(
        string streamId,
        Func<QueuedVideoStreamSegmentConsumerContext, Task<TResult>> actionAsync,
        CancellationToken cancellationToken = default)
    {
        var context = GetOrAddContext(streamId);

        await context.ConsumeLock.WaitAsync(cancellationToken);

        try
        {
            return await actionAsync(context);
        }
        finally
        {
            context.ConsumeLock.Release();
        }
    }

    private QueuedVideoStreamSegmentConsumerContext GetOrAddContext(string streamId) =>
        _contexts.GetOrAdd(
            streamId,
            streamId => new(_hlsOptions.Value.LiveCacheRetention + _hlsOptions.Value.LiveMinSegmentsCount)
            {
                StreamId = streamId,
            });
}

public sealed class QueuedVideoStreamSegmentConsumerContext
{
    public string StreamId { get; init; }
    public SemaphoreSlim ConsumeLock { get; } = new(1, 1);
    public Queue<IVideoStreamSegment> Cache { get; }
    public long Sequence { get; set; }

    public QueuedVideoStreamSegmentConsumerContext(int queueSize) =>
        Cache = new Queue<IVideoStreamSegment>(queueSize);
}
