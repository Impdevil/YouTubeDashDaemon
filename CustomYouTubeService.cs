using System.Threading.Channels;

namespace YT_APP.Services;


/// <summary>
/// for extracting data from the YouTube API
/// </summary>
public class CustomYouTubeService
{
    private readonly ILogger<IYouTubeAPI> _logger;
    private readonly IYouTubeAPI _youTubeAPI;
    public CustomYouTubeService(ILogger<IYouTubeAPI> logger, IYouTubeAPI youTubeAPI)
    {
        _youTubeAPI = youTubeAPI;
        _logger = logger;
    }


    public Task FetchVideosAsync()
    {
        // Simulate fetching videos
        _logger.LogInformation("Fetching videos at: {time}", DateTimeOffset.Now);
        return Task.Delay(1000);
    }

    public async Task<string> GetChannelIDFromHandle(string handle)
    {
        // Simulate creating a playlist
        _logger.LogInformation("Creating playlist at: {time}", DateTimeOffset.Now);
        return await _youTubeAPI.GetChannelIDFromHandleAsync(handle);
        //return Task.FromResult("Playlist ID");
    }

    public async Task<string> GetNewestVideoAsync(string channelID)
    {
        // Simulate getting the newest video
        _logger.LogInformation("Getting newest video at: {time}", DateTimeOffset.Now);
        return await _youTubeAPI.GetNewestVideoAsync(channelID);
        //return Task.FromResult("Newest Video Title");
    }

    public async Task<string> GetHandleLastestVideo(string handle)
    {
        var results = await GetChannelIDFromHandle(handle);
        var channelID = results;
        results = await GetNewestVideoAsync(channelID);
        return results;
    }

    public async Task<string> CreatePlaylist(string title, string description)
    {
        // Simulate creating a playlist
        _logger.LogInformation("Creating playlist at: {time}", DateTimeOffset.Now);
        return await _youTubeAPI.CreatePlaylistAsync(title, description);
        //return Task.FromResult("Playlist ID");
    }


}