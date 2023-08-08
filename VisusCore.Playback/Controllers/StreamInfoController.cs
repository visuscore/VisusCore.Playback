using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using VisusCore.Playback.Services;

namespace VisusCore.Playback.Controllers;

[Route("playback/stream-info")]
public class StreamInfoController : Controller
{
    private readonly StreamsService _streamsService;

    public StreamInfoController(StreamsService streamsService) =>
        _streamsService = streamsService;

    [HttpGet("streams")]
    public async Task<IActionResult> GetStreams() =>
        Json(await _streamsService.GetStreamsAsync());

    [HttpGet("{streamId}")]
    public async Task<IActionResult> GetStream(string streamId)
    {
        var stream = (await _streamsService.GetStreamsAsync(new[] { streamId }))
            .FirstOrDefault();
        if (stream is null)
        {
            return NotFound();
        }

        return Json(stream);
    }
}
