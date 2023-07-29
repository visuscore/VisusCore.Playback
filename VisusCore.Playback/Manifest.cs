using OrchardCore.Modules.Manifest;
using SignalRFeatureIds = VisusCore.SignalR.Constants.FeatureIds;

[assembly: Module(
    Name = "VisusCore Playback",
    Author = "VisusCore",
    Version = "0.0.1",
    Description = "Core playback module.",
    Category = "VisusCore",
    Website = "https://github.com/visuscore/VisusCore.Playback",
    Dependencies = new[]
    {
        SignalRFeatureIds.Module,
    }
)]
