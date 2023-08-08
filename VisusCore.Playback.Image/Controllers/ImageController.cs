using Lombiq.HelpfulLibraries.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using SixLabors.ImageSharp.Formats;
using System.IO;
using System.Threading.Tasks;
using VisusCore.Playback.Image.Models;
using VisusCore.Playback.Image.Services;
using ImageSharpImage = SixLabors.ImageSharp.Image;

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

        return await ServeImageAsync(resolved.Value);
    }

    [HttpGet("{streamId}/{timestampUtc}/details")]
    public async Task<IActionResult> GetSegmentDetailsAsync(string streamId, long timestampUtc)
    {
        var streamDetails = await _imageResolver.GetSegmentDetailsAsync(
            streamId,
            timestampUtc,
            HttpContext.RequestAborted);
        if (streamDetails is null)
        {
            return NotFound();
        }

        return Json(streamDetails);
    }

    [HttpGet("{streamId}/latest")]
    public async Task<IActionResult> GetLatestImageAsync(
        string streamId,
        [FromJsonQueryString] ImageTransformationsParameters transformations)
    {
        var resolved = await _imageResolver.GetLatestImageAsync(
            streamId,
            transformations,
            HttpContext.RequestAborted);
        if (resolved is null)
        {
            return NotFound();
        }

        return await ServeImageAsync(resolved.Value);
    }

    [HttpGet("{streamId}/latest/details")]
    public async Task<IActionResult> GetLatestSegmentDetailsAsync(string streamId)
    {
        var streamDetails = await _imageResolver.GetLatestSegmentDetailsAsync(
            streamId,
            HttpContext.RequestAborted);
        if (streamDetails is null)
        {
            return NotFound();
        }

        return Json(streamDetails);
    }

    private async Task<IActionResult> ServeImageAsync((ImageSharpImage Image, IImageEncoder Encoder) resolved)
    {
        // The stream will be disposed by the FileResult.
#pragma warning disable CA2000 // Dispose objects before losing scope
        var stream = new RecyclableMemoryStream(MemoryStreamManager);
#pragma warning restore CA2000 // Dispose objects before losing scope
        try
        {
            var (image, formatEncoder) = resolved;
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
