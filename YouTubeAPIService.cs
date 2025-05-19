using System.Threading.Channels;
using Google;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YoutubeExplode;
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


    /// <summary>
    /// uses the YoutubeExplode library to get the channel ID from the handle of a channel the part after the @
    /// </summary>
    /// <param name="handle">the channel vanity url after the @</param>
    /// <returns></returns>
    public async Task<Channel> GetChannelFromHandleAsync(string handle)
    {
        _logger.LogInformation("Getting newest video at: {0}", DateTimeOffset.Now);
        _logger.LogInformation("Channel url: {0}", handle);

        var youtubeClient = new YoutubeClient();

        try
        {

            var channel = await youtubeClient.Channels.GetByHandleAsync(handle);
            _logger.LogInformation("fetching channel data from url: {0} , {1}", channel.Id, channel.Title);
            return new Channel
            {
                ChannelID = channel.Id,
                Handle = channel.Title,
                Tags = string.Empty, // Placeholder for tags
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching channel ID for URL: {0}", handle);
            return new Channel
            {
                ChannelID = "Error fetching channel ID" + ex.Message,
                Handle = "FAIL",
                Tags = string.Empty, // Placeholder for tags
            };
        }
    }

    public async Task<YTvideo> GetNewestVideoAsync(string channelId)
    {
        var youtubeClient = new YoutubeClient();
        YTvideo latestSavedUploaded = new YTvideo();
        try
        {
            var videos = youtubeClient.Channels.GetUploadsAsync(channelId);


            await foreach (var latestVideo in videos)
            {
                _logger.LogInformation("Video ID: {0}", latestVideo.Id);
                _logger.LogInformation("Video Title: {0}", latestVideo.Title);
                latestSavedUploaded = new YTvideo
                {
                    VideoID = latestVideo.Id,
                    Title = latestVideo.Title,
                    ChannelID = channelId,
                    Description = "Unknown",
                    PublishedAt = DateTime.UtcNow
                };
                break;
            }

            try
            {
                var videoDetails = await youtubeClient.Videos.GetAsync(latestSavedUploaded.VideoID);

                latestSavedUploaded.Description = videoDetails.Description;
                latestSavedUploaded.PublishedAt = videoDetails.UploadDate.DateTime;
                return latestSavedUploaded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching video details for ID: {0}", latestSavedUploaded.VideoID);
                return new YTvideo
                {
                    VideoID = "fetching video details failed" + ex.Message,
                    Title = "FAIL",
                    ChannelID = channelId,
                    Description = "FAIL",
                    PublishedAt = DateTime.MinValue
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest video for channel: {0}", channelId);
            return new YTvideo
            {
                VideoID = "FAIL",
                Title = "FAIL",
                ChannelID = channelId,
                Description = "FAIL",
                PublishedAt = DateTime.MinValue
            };
        }


    }

}