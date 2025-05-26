using System.ComponentModel;
using System.Threading.Channels;
namespace YT_APP.Services;

using YT_APP.Database;
using YT_APP.ServiceStructs;


/// <summary>
/// for extracting data from the YouTube API
/// </summary>
public class CustomYouTubeService
{
    private readonly ILogger<CustomYouTubeService> _logger;
    private readonly IYouTubeAPI _youTubeAPI;
    private readonly DatabaseHelper _databaseHelper;
    public CustomYouTubeService(ILogger<CustomYouTubeService> logger, IYouTubeAPI youTubeAPI, DatabaseHelper databaseHelper)
    {
        _databaseHelper = databaseHelper;
        _youTubeAPI = youTubeAPI;
        _logger = logger;
    }


    public async Task<Channel> GetChannelFromHandle(string handle)
    {
        _logger.LogInformation("Getting Channel at: {0}", DateTime.UtcNow);
        return await _youTubeAPI.GetChannelFromHandleAsync(handle);

    }

    public async Task<ServiceStructs.Video> GetNewestVideoAsync(string channelID)
    {
        _logger.LogInformation("Getting newest video at: {0}", DateTime.UtcNow);
        return await _youTubeAPI.GetNewestVideoAsync(channelID);
    }

    public async Task<ServiceStructs.Video> GetHandleLastestVideo(string handle)
    {
        var results = await GetChannelFromHandle(handle);
        var channelID = results.ChannelID;
        var resultsVideo = await GetNewestVideoAsync(channelID);
        return resultsVideo;
    }


    public async Task<string> AddChannelToCheckList(string handle, string tags)
    {
        var results = await GetChannelFromHandle(handle);
        var channelID = results.ChannelID;
        _logger.LogInformation("Adding channel to check list: {0}", channelID);
        Channel channel = new Channel
        {
            ChannelID = results.ChannelID,
            Handle = results.Handle,
            Tags = tags,
            LastChecked = DateTime.UtcNow.AddDays(-1)
        };
        try
        {
            // Add the channel to the database
            _databaseHelper.InsertChannel(channel);
            _logger.LogInformation("Added channel to check list: {0}", channelID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding channel to check list: {0}", channelID);
            return "Failed, DB ERROR";
        }

        return "SUCCESS";
    }

}