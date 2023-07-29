using OrchardCore.Modules.Manifest;
using ConsumerFeatureIds = VisusCore.Consumer.Constants.FeatureIds;
using PlaybackFeatureIds = VisusCore.Playback.Constants.FeatureIds;
using StorageFeatureIds = VisusCore.Storage.Constants.FeatureIds;

[assembly: Module(
    Name = "VisusCore Hls Playback",
    Author = "VisusCore",
    Version = "0.0.1",
    Description = "Playback module to provide HLS playlists from the consumed streams.",
    Category = "VisusCore",
    Website = "https://github.com/visuscore/VisusCore.Playback",
    Dependencies = new[]
    {
        ConsumerFeatureIds.Module,
        PlaybackFeatureIds.Module,
        StorageFeatureIds.Module,
    }
)]
