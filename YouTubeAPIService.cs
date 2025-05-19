using System.Threading.Channels;
using Google;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
namespace YT_APP.Services;


public struct YTvideo
{
    public string VideoID { get; set; }
    public string ChannelID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime PublishedAt { get; set; }

}
public struct Channel
{
    public string ChannelID { get; set; }
    public string Handle { get; set; }
    public string Tags { get; set; }
}

public struct Playlist
{
    public string PlaylistID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IYouTubeAPI
{
    Task<Channel> GetChannelFromHandleAsync(string handle);
    Task<YTvideo> GetNewestVideoAsync(string channelID);
    Task<Playlist> CreatePlaylistAsync(string title, string description);
}


/// <summary>
/// for lower level API calls
/// </summary>
public class YouTubeAPIService : IYouTubeAPI
{
    private readonly ILogger<IYouTubeAPI> _logger;
    private readonly string _apiKey;
    private readonly string _applicationName;
    private readonly string _clientId;

    public YouTubeAPIService(ILogger<IYouTubeAPI> logger, string apiKey, string applicationName, string clientId)
    {
        _logger = logger;
        _apiKey = apiKey;
        _applicationName = applicationName;
        _clientId = clientId;
    }


    public async Task<Channel> GetChannelFromHandleAsync(string handle)
    {
        // Simulate getting the channel ID from a handle
        _logger.LogInformation("Getting channel ID from handle at: {time}", DateTimeOffset.Now);
        _logger.LogInformation("Handle: {handle}", handle);
        var youTubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = _apiKey,
            ApplicationName = _applicationName
        });
        var channelsListRequest = youTubeService.Channels.List("id");
        channelsListRequest.ForHandle = handle;
        try
        {
            var channelsListResponse = await channelsListRequest.ExecuteAsync();


            _logger.LogInformation("API Response: {0}", channelsListResponse);
            Console.WriteLine("API Response: {0}", channelsListResponse.ToString());

            if (channelsListResponse == null || channelsListResponse.Items == null || channelsListResponse.Items.Count == 0)
            {
                _logger.LogWarning("No channel found for handle: {handle}", handle);
                return new Channel
                {
                    ChannelID = "No channel found for handle",
                    Handle = "FAIL",
                    Tags = string.Empty, // Placeholder for tags
                };
            }
            var channel = channelsListResponse.Items.FirstOrDefault();
            if (channel != null)
            {
                return new Channel
                {
                    ChannelID = channel.Id,
                    Handle = handle,
                    Tags = string.Empty, // Placeholder for tags
                };
            }
            else
            {
                _logger.LogWarning("No channel found for handle: {handle}", handle);
                return new Channel
                {
                    ChannelID = "Error fetching channel ID",
                    Handle = "FAIL",
                    Tags = string.Empty, // Placeholder for tags
                };
            }
        }
        catch (GoogleApiException ex)
        {
            _logger.LogError(ex, "Google API error fetching channel ID for handle: {handle}", handle);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching channel ID for handle: {handle}", handle);
            throw;
        }

    }


    public async Task<YTvideo> GetNewestVideoAsync(string channelID)
    {
        // Simulate getting the newest video
        _logger.LogInformation("Getting newest video at: {time}", DateTimeOffset.Now);
        _logger.LogInformation("Channel ID: {channelID}", channelID);
        var youTubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = _apiKey,
            ApplicationName = _applicationName
        });

        var channelListRequest = youTubeService.Channels.List("contentDetails");
        channelListRequest.Id = channelID;
        channelListRequest.Mine = false;
        channelListRequest.MaxResults = 1;

        using (var stream = await channelListRequest.ExecuteAsStreamAsync())
        using (var reader = new StreamReader(stream))
        {
            var rawJson = await reader.ReadToEndAsync();
            _logger.LogInformation("Raw JSON Response: {0}", rawJson);
            Console.WriteLine("Raw JSON Response: {0}", rawJson);
        }
        var channelListResponse = await channelListRequest.ExecuteAsync();


        var newestVideo = channelListResponse.Items.Where(item => item.ContentDetails != null)
            .Select(item => item.ContentDetails)
            .Select(item => item.RelatedPlaylists)
            .SelectMany(playlist => playlist.Uploads)
            .FirstOrDefault();

        if (newestVideo != null)
        {

            return new YTvideo
            {
                VideoID = newestVideo.ToString(),
                ChannelID = channelID,
                Title = "Sample Title", // Placeholder for title
                Description = "Sample Description", // Placeholder for description
                PublishedAt = DateTime.UtcNow // Placeholder for published date
            };  
        }
        else
        {
            _logger.LogWarning("No videos found for channel ID: {channelID}", channelID);
            return new YTvideo
            {
                VideoID = "No videos found for channel ID",
                ChannelID = channelID,
                Title = "FAIL",
                Description = string.Empty,
                PublishedAt = DateTime.UtcNow
            };
        }
    }
    public async Task<Playlist> CreatePlaylistAsync(string title, string description)
    {
        // Simulate creating a playlist
        _logger.LogInformation("Creating playlist at: {time}", DateTimeOffset.Now);




        return  new Playlist
        {
            PlaylistID = "SamplePlaylistID", // Placeholder for playlist ID
            Name = title,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

}