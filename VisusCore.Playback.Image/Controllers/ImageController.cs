using Lombiq.HelpfulLibraries.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using System.IO;
using System.Threading.Tasks;
using VisusCore.Playback.Image.Models;
using VisusCore.Playback.Image.Services;

namespace VisusCore.Playback.Image.Controllers;

[Route("playback/image")]
public class ImageController : Controller
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
    private readonly ImageResolverService _imageResolver;

    public ImageController(ImageResolverService imageResolver) =>
        _imageResolver = imageResolver;

    [HttpGet("{streamId}/{timestampUtc}")]
    public async Task<IActionResult> GetImageAsync(
        string streamId,
        long timestampUtc,
        bool exact,
        [FromJsonQueryString] ImageTransformationsParameters transformations)
    {
        var resolved = await _imageResolver.GetImageAsync(
            streamId,
            timestampUtc,
            exact,
            transformations,
            HttpContext.RequestAborted);
        if (resolved is null)
        {
            return NotFound();
        }

        // The stream will be disposed by the FileResult.
#pragma warning disable CA2000 // Dispose objects before losing scope
        var stream = new RecyclableMemoryStream(MemoryStreamManager);
#pragma warning restore CA2000 // Dispose objects before losing scope
        try
        {
            var (image, formatEncoder) = resolved.Value;
            using (image)
            {
                await image.SaveAsync(stream, formatEncoder, HttpContext.RequestAborted);

                stream.Seek(0, SeekOrigin.Begin);

                return File(stream, "image/jpeg");
            }
        }
        catch
        {
            await stream.DisposeAsync();

            throw;
        }
    }
}
