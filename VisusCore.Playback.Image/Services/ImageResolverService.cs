using Microsoft.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VisusCore.Native.Ffmpeg.Core.FrameExtractor;
using VisusCore.Playback.Image.Extensions;
using VisusCore.Playback.Image.Models;
using VisusCore.Storage.Abstractions.Services;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace VisusCore.Playback.Image.Services;

public class ImageResolverService
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
    private readonly IStreamSegmentStorageReader _storageReader;

    public ImageResolverService(IStreamSegmentStorageReader storageReader) =>
        _storageReader = storageReader;

    public async Task<(ImageSharpImage Image, IImageEncoder Encoder)?> GetImageAsync(
        string streamId,
        long timestampUtc,
        bool exact,
        ImageTransformationsParameters transformations,
        CancellationToken cancellationToken = default)
    {
        var segment = await _storageReader.GetSegmentAroundAsync(streamId, timestampUtc, cancellationToken);
        if (segment is null)
        {
            return null;
        }

        using var segmentStream = new RecyclableMemoryStream(MemoryStreamManager);
        segmentStream.Write(segment.Init.Init);
        segmentStream.Write(segment.Data);
        segmentStream.Seek(0, SeekOrigin.Begin);

        using var extractor = new Extractor(segmentStream);

        var segmentFrames = new List<Frame>();
        while (extractor.TryReadNext(out var frame))
        {
            segmentFrames.Add(frame);
            if (frame is VideoFrame videoFrame)
            {
                return ProcessImage(videoFrame, transformations);
            }
        }

        return null;
    }

    private static (ImageSharpImage Image, IImageEncoder Encoder) ProcessImage(
        VideoFrame videoFrame,
        ImageTransformationsParameters transformations)
    {
        transformations ??= new ImageTransformationsParameters();
        transformations.Quality ??= 75;

        var image = ImageSharpImage.LoadPixelData<Rgb24>(videoFrame.Data, videoFrame.Width, videoFrame.Height);

        if (transformations.Scale is not null and not 1)
        {
            image.Mutate(processingContext =>
                processingContext.Resize(
                    Convert.ToInt32(image.Width * transformations.Scale.Value),
                    Convert.ToInt32(image.Height * transformations.Scale.Value)));
        }

        if (transformations.HasCrop())
        {
            image.Mutate(processingContext =>
                processingContext.Crop(
                new Rectangle(
                    Convert.ToInt32(image.Width * transformations.CropLeft.Value),
                    Convert.ToInt32(image.Height * transformations.CropTop.Value),
                    Convert.ToInt32(image.Width * (transformations.CropRight.Value - transformations.CropLeft.Value)),
                    Convert.ToInt32(image.Height * (transformations.CropBottom.Value - transformations.CropTop.Value)))));
        }

        var formatEncoder = (JpegEncoder)image.GetConfiguration()
            .ImageFormatsManager
            .GetEncoder(JpegFormat.Instance);
        if (formatEncoder.Quality != transformations.Quality.Value)
        {
            formatEncoder = new JpegEncoder
            {
                Quality = transformations.Quality.Value,
                Interleaved = formatEncoder.Interleaved,
                ColorType = formatEncoder.ColorType,
                SkipMetadata = formatEncoder.SkipMetadata,
            };
        }

        return (Image: image, Encoder: formatEncoder);
    }
}
