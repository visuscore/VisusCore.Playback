using OrchardCore.Modules.Manifest;
using PlaybackFeatureIds = VisusCore.Playback.Constants.FeatureIds;

[assembly: Module(
    Name = "VisusCore Hls Playback",
    Author = "VisusCore",
    Version = "0.0.1",
    Description = "Playback module to provide HLS playlists from the consumed streams.",
    Category = "VisusCore",
    Website = "https://github.com/visuscore/VisusCore.Playback",
    Dependencies = new[]
    {
        PlaybackFeatureIds.Module,
    }
)]
