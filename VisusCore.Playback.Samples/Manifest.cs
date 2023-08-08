using OrchardCore.Modules.Manifest;
using HlsPlaybackFeatureIds = VisusCore.Playback.Hls.Constants.FeatureIds;
using ImagePlaybackFeatureIds = VisusCore.Playback.Image.Constants.FeatureIds;

[assembly: Module(
    Name = "VisusCore Playback - Samples",
    Author = "VisusCore",
    Version = "0.0.1",
    Description = "Playback samples.",
    Category = "VisusCore",
    Website = "https://github.com/visuscore/VisusCore.Playback",
    Dependencies = new[]
    {
        HlsPlaybackFeatureIds.Module,
        ImagePlaybackFeatureIds.Module,
    }
)]
