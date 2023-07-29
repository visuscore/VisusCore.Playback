using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using System;
using VisusCore.Playback.Image.Hubs;
using VisusCore.Playback.Image.Services;

namespace VisusCore.Playback.Image;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<ImageResolverService>();

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
        app.UseEndpoints(endpoints =>
            endpoints.MapHub<ImageHub>("playback/image"));
}
