using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisusCore.Configuration.VideoStream.Core.Models;
using VisusCore.Playback.Models;
using YesSql;
using YesSql.Services;

namespace VisusCore.Playback.Services;

public class StreamsService
{
    private readonly ISession _session;

    public StreamsService(ISession session) =>
        _session = session;

    public async Task<IEnumerable<StreamInfo>> GetStreamsAsync(IEnumerable<string> streamIds = default)
    {
        var query = _session.QueryIndex<StreamEntityPartIndex>()
            .Where(index => index.Latest && index.Published);
        if (streamIds != null)
        {
            query = query.Where(index => index.ContentItemId.IsIn(streamIds));
        }

        return (await query.ListAsync())
            .Select(index => new StreamInfo
            {
                Id = index.ContentItemId,
                Enabled = index.Enabled,
                Name = index.Name,
            })
            .ToArray();
    }
}
