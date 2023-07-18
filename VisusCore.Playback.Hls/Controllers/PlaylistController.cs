using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Core.Utilities;
using PlaylistsNET.Content;
using PlaylistsNET.Models;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisusCore.Playback.Hls.Constants;
using VisusCore.Playback.Hls.Models;
using VisusCore.Playback.Hls.Services;

namespace VisusCore.Playback.Hls.Controllers;

[Route("playback/hls")]
public class PlaylistController : Controller
{
    private readonly IOptions<HlsOptions> _hlsOptions;
    private readonly QueuedVideoStreamSegmentConsumerContextAccessor _contextAccessor;
    private readonly LinkGenerator _linkGenerator;

    public PlaylistController(
        IOptions<HlsOptions> hlsOptions,
        QueuedVideoStreamSegmentConsumerContextAccessor contextAccessor,
        LinkGenerator linkGenerator)
    {
        _hlsOptions = hlsOptions;
        _contextAccessor = contextAccessor;
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
                    context.Cache.OrderBy(segment => segment.Metadata.TimestampUtc)
                        .TakeLast(_hlsOptions.Value.LiveMinSegmentsCount)
                        .ToList(),
                    context.Sequence)),
            HttpContext.RequestAborted);

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
                context.Cache.FirstOrDefault(segment => segment.Metadata.TimestampUtc == timestampUtc)),
            HttpContext.RequestAborted);

        if (segment == null)
        {
            return NotFound();
        }

        var segmentStream = new MemoryStream();
        segmentStream.Write(segment.Init.Init);
        segmentStream.Write(segment.Data);
        segmentStream.Seek(0, SeekOrigin.Begin);

        return File(segmentStream, "application/octet-stream");
    }
}
