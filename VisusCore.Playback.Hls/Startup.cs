using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using VisusCore.Consumer.Core.Extensions;
using VisusCore.Playback.Hls.Models;
using VisusCore.Playback.Hls.Services;

namespace VisusCore.Playback.Hls;

public class Startup : StartupBase
{
    private const string HlsSection = "VisusCore_Playback_Hls";
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<HlsOptions>(
            options => _shellConfiguration
                .GetSection($"{HlsSection}:{nameof(HlsOptions)}")
                .Bind(options));
        services.AddVideoStreamSegmentConsumer<QueuedVideoStreamSegmentConsumer>();
        services.AddSingleton<QueuedVideoStreamSegmentConsumerContextAccessor>();
    }
}
