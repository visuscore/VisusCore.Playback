using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using System;
using VisusCore.Playback.Hubs;
using VisusCore.Playback.Services;

namespace VisusCore.Playback;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<StreamsService>();

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
        app.UseEndpoints(endpoints =>
            endpoints.MapHub<StreamInfoHub>("playback/stream-info"));
}
