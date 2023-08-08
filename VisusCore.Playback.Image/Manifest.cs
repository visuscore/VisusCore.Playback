using OrchardCore.Modules.Manifest;
using FfmpegFeatureIds = VisusCore.Ffmpeg.Constants.FeatureIds;
using PlaybackFeatureIds = VisusCore.Playback.Constants.FeatureIds;

[assembly: Module(
    Name = "VisusCore Image Playback",
    Author = "VisusCore",
    Version = "0.0.1",
    Description = "Playback module to provide images from the consumed streams.",
    Category = "VisusCore",
    Website = "https://github.com/visuscore/VisusCore.Playback",
    Dependencies = new[]
    {
        FfmpegFeatureIds.Module,
        PlaybackFeatureIds.Module,
    }
)]
