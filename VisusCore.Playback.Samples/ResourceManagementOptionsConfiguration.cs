using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using System;
using VisusCore.Playback.Samples.Constants;

namespace VisusCore.Playback.Samples;

public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private const string Root = "~/" + FeatureIds.Area;
    private const string Scripts = Root + "/js";

    private static readonly ResourceManifest _manifest = new();

    static ResourceManagementOptionsConfiguration() =>
        _manifest
            .DefineScript(ResourceNames.PlaybackSampleApp)
            .SetUrl(Scripts + "/App.min.js", Scripts + "/App.js");

    public void Configure(ResourceManagementOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.ResourceManifests.Add(_manifest);
    }
}
