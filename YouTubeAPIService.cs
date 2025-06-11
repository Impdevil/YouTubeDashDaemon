using System.Threading.Channels;
using Google;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YoutubeExplode;
using YT_APP.ServiceStructs;
using YT_APP.Database;
namespace YT_APP.Services;



public interface IYouTubeAPI
{

    Task<ServiceStructs.Channel> GetChannelFromHandleAsync(string handle);

    Task<ServiceStructs.Video> GetNewestVideoAsync(string channelID);

    Task<List<ServiceStructs.Video>> GetNewestListVideosAsync(string channelId, int count = 1);
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
    public async Task<ServiceStructs.Channel> GetChannelFromHandleAsync(string handle)
    {
        _logger.LogInformation("Getting newest video at: {0}", DateTimeOffset.Now);
        _logger.LogInformation("Channel url: {0}", handle);

        var youtubeClient = new YoutubeClient();

        try
        {

            var channel = await youtubeClient.Channels.GetByHandleAsync(handle);
            _logger.LogInformation("fetching channel data from url: {0} , {1}", channel.Id, channel.Title);
            return new ServiceStructs.Channel
            {
                ChannelID = channel.Id,
                Handle = channel.Title,
                Tags = string.Empty, // Placeholder for tags
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching channel ID for URL: {0}", handle);
            return new ServiceStructs.Channel
            {
                ChannelID = "Error fetching channel ID" + ex.Message,
                Handle = "FAIL",
                Tags = string.Empty, // Placeholder for tags
            };
        }
    }

    public async Task<ServiceStructs.Video> GetNewestVideoAsync(string channelId)
    {
        var youtubeClient = new YoutubeClient();
        ServiceStructs.Video latestSavedUploaded = new ServiceStructs.Video();
        try
        {
            var videos = youtubeClient.Channels.GetUploadsAsync(channelId);


            await foreach (var latestVideo in videos)
            {
                _logger.LogInformation("Video ID: {0}", latestVideo.Id);
                _logger.LogInformation("Video Title: {0}", latestVideo.Title);
                latestSavedUploaded = new ServiceStructs.Video
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
                return new ServiceStructs.Video
                {
                    VideoID = "fetching video details failed." + ex.Message,
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
            return new ServiceStructs.Video
            {
                VideoID = "FAIL",
                Title = "FAIL",
                ChannelID = channelId,
                Description = "FAIL",
                PublishedAt = DateTime.MinValue
            };
        }


    }
    public async Task<List<ServiceStructs.Video>> GetNewestListVideosAsync(string channelId, int count = 1)
    {
        var youtubeClient = new YoutubeClient();
        List<ServiceStructs.Video> latestSavedUploaded = new List<ServiceStructs.Video>();
        try
        {
            var videos = youtubeClient.Channels.GetUploadsAsync(channelId);

            await foreach (var latestVideo in videos)
            {
                _logger.LogInformation("Video ID: {0}", latestVideo.Id);
                _logger.LogInformation("Video Title: {0}", latestVideo.Title);
                latestSavedUploaded.Add(new ServiceStructs.Video
                {
                    VideoID = latestVideo.Id,
                    Title = latestVideo.Title,
                    ChannelID = channelId,
                    Description = "Unknown",
                    PublishedAt = DateTime.UtcNow
                });
                if (count > 0 && --count <= 0)
                {
                    break;
                }
            }

            try    
            {   
                for (int i = 0; i < latestSavedUploaded.Count; i++)
                {
                    _logger.LogInformation("Fetching details for video ID: {0}", latestSavedUploaded[i].VideoID);
                    var toUpdate = latestSavedUploaded[i];
                    var videoDetails = await youtubeClient.Videos.GetAsync(latestSavedUploaded[i].VideoID);

                    toUpdate.Description = videoDetails.Description;
                    toUpdate.PublishedAt = videoDetails.UploadDate.DateTime;
                    toUpdate.Duration = videoDetails.Duration?.ToString() ?? "Unknown";
                    latestSavedUploaded[i] = toUpdate;
                }
                if (latestSavedUploaded.Count == 0)
                {
                    _logger.LogWarning("No videos found for channel: {0}", channelId);
                    return new List<ServiceStructs.Video>
                    { new ServiceStructs.Video
                        {
                        VideoID = "No videos found",
                        Title = "No videos",
                        ChannelID = channelId,
                        Description = "No videos found",
                        PublishedAt = DateTime.MinValue
                        }
                    };
                }
                _logger.LogInformation("Fetched {0} videos for channel: {1}", latestSavedUploaded.Count, channelId);
                return latestSavedUploaded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching video details for channel: {0}", channelId);
                return new List<ServiceStructs.Video>
                { new ServiceStructs.Video
                    {
                    VideoID = "fetching video details failed." + ex.Message,
                    Title = "FAIL",
                    ChannelID = channelId,
                    Description = "FAIL",
                    PublishedAt = DateTime.MinValue
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest video for channel: {0}", channelId);
            return new List<ServiceStructs.Video>
                { new ServiceStructs.Video
                    {

                VideoID = "FAIL",
                Title = "FAIL",
                ChannelID = channelId,
                Description = "FAIL",
                PublishedAt = DateTime.MinValue
                }
            };
        }


    }

}