using Lombiq.HelpfulLibraries.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IO;
using System.IO;
using System.Threading.Tasks;
using VisusCore.Playback.Image.Models;
using VisusCore.Playback.Image.Services;

namespace VisusCore.Playback.Image.Hubs;

public class ImageHub : Hub
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
    private readonly ImageResolverService _imageResolver;

    public ImageHub(ImageResolverService imageResolver) =>
        _imageResolver = imageResolver;

    public async Task<byte[]> GetImageAsync(
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
            Context.ConnectionAborted);
        if (resolved is null)
        {
            return null;
        }

        using var stream = new RecyclableMemoryStream(MemoryStreamManager);
        var (image, formatEncoder) = resolved.Value;
        using (image)
        {
            await image.SaveAsync(stream, formatEncoder, Context.ConnectionAborted);

            stream.Seek(0, SeekOrigin.Begin);

            return stream.GetSpan().ToArray();
        }
    }
}
