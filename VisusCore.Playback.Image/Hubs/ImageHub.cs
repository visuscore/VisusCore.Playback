using Microsoft.AspNetCore.SignalR;
using Microsoft.IO;
using SixLabors.ImageSharp.Formats;
using System.IO;
using System.Threading.Tasks;
using VisusCore.Native.Ffmpeg.Core.Models;
using VisusCore.Playback.Image.Models;
using VisusCore.Playback.Image.Services;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace VisusCore.Playback.Image.Hubs;

public class ImageHub : Hub
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
    private readonly ImageResolverService _imageResolver;

    public ImageHub(ImageResolverService imageResolver) =>
        _imageResolver = imageResolver;

    public Task<StreamDetails> GetLatestSegmentDetailsAsync(string streamId) =>
        _imageResolver.GetLatestSegmentDetailsAsync(streamId, Context.ConnectionAborted);

    public Task<StreamDetails> GetSegentDetailsAsync(string streamId, long timestampUtc) =>
        _imageResolver.GetSegmentDetailsAsync(streamId, timestampUtc, Context.ConnectionAborted);

    public async Task<byte[]> GetImageAsync(
        string streamId,
        long timestampUtc,
        bool exact,
        ImageTransformationsParameters transformations)
    {
        var resolved = await _imageResolver.GetImageAsync(
            streamId,
            timestampUtc,
            exact,
            transformations,
            Context.ConnectionAborted);

        return await ServeImageAsync(resolved);
    }

    public async Task<byte[]> GetLatestImageAsync(
        string streamId,
        ImageTransformationsParameters transformations)
    {
        var resolved = await _imageResolver.GetLatestImageAsync(
            streamId,
            transformations,
            Context.ConnectionAborted);

        return await ServeImageAsync(resolved);
    }

    public async Task<byte[]> ServeImageAsync((ImageSharpImage Image, IImageEncoder Encoder)? resolved)
    {
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
