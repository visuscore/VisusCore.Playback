using OrchardCore.Modules.Manifest;
using ConsumerFeatureIds = VisusCore.Consumer.Constants.FeatureIds;
using SignalRFeatureIds = VisusCore.SignalR.Constants.FeatureIds;
using StorageFeatureIds = VisusCore.Storage.Constants.FeatureIds;

[assembly: Module(
    Name = "VisusCore Playback",
    Author = "VisusCore",
    Version = "0.0.1",
    Description = "Core playback module.",
    Category = "VisusCore",
    Website = "https://github.com/visuscore/VisusCore.Playback",
    Dependencies = new[]
    {
        ConsumerFeatureIds.Module,
        SignalRFeatureIds.Module,
        StorageFeatureIds.Module,
    }
)]
