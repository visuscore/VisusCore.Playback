using Microsoft.AspNetCore.Mvc;

namespace VisusCore.Playback.Samples.Controllers;

[Route("playback/sample")]
public class PlaybackSampleController : Controller
{
    public IActionResult Index() => View();
}
