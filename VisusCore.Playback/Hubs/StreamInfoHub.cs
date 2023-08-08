using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisusCore.Playback.Models;
using VisusCore.Playback.Services;

namespace VisusCore.Playback.Hubs;

public class StreamInfoHub : Hub
{
    private readonly StreamsService _streamsService;

    public StreamInfoHub(StreamsService streamsService) =>
        _streamsService = streamsService;

    public Task<IEnumerable<StreamInfo>> GetStreamsAsync() =>
        _streamsService.GetStreamsAsync();

    public async Task<StreamInfo> GetStreamAsync(string streamId) =>
        (await _streamsService.GetStreamsAsync(new[] { streamId }))
            .FirstOrDefault()
            ?? throw new HubException($"Stream with id {streamId} not found");
}
