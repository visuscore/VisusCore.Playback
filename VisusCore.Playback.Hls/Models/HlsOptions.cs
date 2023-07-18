namespace VisusCore.Playback.Hls.Models;

public class HlsOptions
{
    public int LiveMinSegmentsCount { get; set; } = 5;
    public int LiveCacheRetention { get; set; } = 10;
}
