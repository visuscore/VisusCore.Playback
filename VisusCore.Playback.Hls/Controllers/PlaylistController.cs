using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using OrchardCore.Mvc.Core.Utilities;
using PlaylistsNET.Content;
using PlaylistsNET.Models;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisusCore.Consumer.Abstractions.Models;
using VisusCore.Playback.Hls.Constants;
using VisusCore.Playback.Hls.Models;
using VisusCore.Playback.Hls.Services;
using VisusCore.Storage.Abstractions.Services;

namespace VisusCore.Playback.Hls.Controllers;

[Route("playback/hls")]
public class PlaylistController : Controller
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
    private readonly IOptions<HlsOptions> _hlsOptions;
    private readonly QueuedVideoStreamSegmentConsumerContextAccessor _contextAccessor;
    private readonly IStreamSegmentStorageReader _storageReader;
    private readonly LinkGenerator _linkGenerator;

    public PlaylistController(
        IOptions<HlsOptions> hlsOptions,
        QueuedVideoStreamSegmentConsumerContextAccessor contextAccessor,
        IStreamSegmentStorageReader storageReader,
        LinkGenerator linkGenerator)
    {
        _hlsOptions = hlsOptions;
        _contextAccessor = contextAccessor;
        _storageReader = storageReader;
        _linkGenerator = linkGenerator;
    }

    [HttpGet("{streamId}/live/playlist")]
    public async Task<IActionResult> Live(string streamId)
    {
        if (!_contextAccessor.IsExists(streamId))
        {
            return NotFound();
        }

        var (segments, sequence) = await _contextAccessor.InvokeLockedAsync(
            streamId,
            context => Task.FromResult(
                (
                    context.Queue.OrderBy(segment => segment.Metadata.TimestampUtc)
                        .TakeLast(_hlsOptions.Value.LiveMinSegmentsCount)
                        .ToList(),
                    context.Sequence)),
            cancellationToken: HttpContext.RequestAborted);

        if (!segments.Any())
        {
            return NotFound();
        }

        var playlist = new HlsMediaPlaylist
        {
            Version = 11,
            MediaSequence = sequence,
            TargetDuration = (int)(segments.Max(segment => segment.Metadata.Duration) / 1_000_000),
            PlaylistEntries = segments.Select(segment => new HlsMediaPlaylistEntry
            {
                Duration = (int)(segment.Metadata.Duration / 1_000_000),
                Path = _linkGenerator.GetPathByAction(
                    nameof(LiveSegment),
                    typeof(PlaylistController).ControllerName(),
                    new
                    {
                        Area = FeatureIds.Module,
                        StreamId = streamId,
                        segment.Metadata.TimestampUtc,
                    }),
            }).ToList(),
        };

        return File(Encoding.UTF8.GetBytes(PlaylistToTextHelper.ToText(playlist)), "application/vnd.apple.mpegurl");
    }

    [HttpGet("{streamId}/live/segment/{timestampUtc}")]
    public async Task<IActionResult> LiveSegment(string streamId, long timestampUtc)
    {
        if (!_contextAccessor.IsExists(streamId))
        {
            return NotFound();
        }

        var segment = await _contextAccessor.InvokeLockedAsync(
            streamId,
            context => Task.FromResult(
                context.Queue.FirstOrDefault(segment => segment.Metadata.TimestampUtc == timestampUtc)),
            cancellationToken: HttpContext.RequestAborted);

        if (segment == null)
        {
            return NotFound();
        }

        return ServeSegment(segment);
    }

    [HttpGet("{streamId}/playback/playlist/{timestampUtc}")]
    public async Task<IActionResult> Playback(string streamId, long timestampUtc)
    {
        var metas = await _storageReader.GetSegmentMetasAsync(
            streamId, timestampUtc, endTimestampUtc: null, skip: null, 1000, HttpContext.RequestAborted);

        var playlist = new HlsMediaPlaylist
        {
            Version = 11,
            MediaSequence = 0,
            TargetDuration = (int)(metas.Max(meta => meta.Duration) / 1_000_000),
            PlaylistEntries = metas.Select(meta => new HlsMediaPlaylistEntry
            {
                Duration = (int)(meta.Duration / 1_000_000),
                Path = _linkGenerator.GetPathByAction(
                    nameof(PlaybackSegment),
                    typeof(PlaylistController).ControllerName(),
                    new
                    {
                        Area = FeatureIds.Module,
                        StreamId = streamId,
                        meta.TimestampUtc,
                    }),
            }).ToList(),
        };

        return File(Encoding.UTF8.GetBytes(PlaylistToTextHelper.ToText(playlist)), "application/vnd.apple.mpegurl");
    }

    [HttpGet("{streamId}/playback/segment/{timestampUtc}")]
    public async Task<IActionResult> PlaybackSegment(string streamId, long timestampUtc)
    {
        if (!_contextAccessor.IsExists(streamId))
        {
            return NotFound();
        }

        var segments = await _storageReader.GetSegmentsAsync(
            streamId, timestampUtc, endTimestampUtc: null, skip: null, 1, HttpContext.RequestAborted);

        if (segments.FirstOrDefault() is not { } segment)
        {
            return NotFound();
        }

        return ServeSegment(segment);
    }

    private IActionResult ServeSegment(IVideoStreamSegment segment)
    {
        // The stream will be disposed by the FileResult.
#pragma warning disable CA2000 // Dispose objects before losing scope
        var segmentStream = new RecyclableMemoryStream(MemoryStreamManager);
#pragma warning restore CA2000 // Dispose objects before losing scope
        segmentStream.Write(segment.Init.Init);
        segmentStream.Write(segment.Data);
        segmentStream.Seek(0, SeekOrigin.Begin);

        return File(segmentStream, "application/octet-stream");
    }
}
