using System.ComponentModel;
using System.Threading.Channels;
namespace YT_APP.Services;
using YT_APP.Database;


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

    public async Task<string> AddNewChannelToCheck(string handle, string tags)
    {
        // Simulate adding a new channel to check
        _logger.LogInformation("Adding new channel at: {time}", DateTimeOffset.Now);
        Services.Channel channel = await GetChannelFromHandle(handle);

        _logger.LogInformation("Channel ID: {0}", channel.ChannelID);
        _logger.LogInformation("Channel Handle: {0}", channel.ChannelID);

        if (channel.Handle == "FAIL")
        {
            return channel.ChannelID;
        }
        try
        {
            _databaseHelper.InsertChannel(channel.Handle, channel.ChannelID, tags);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error inserting channel into database: {0}", ex);
            return "Error inserting channel into database";
        }
        return channel.ChannelID;
    }


    public async Task CheckAddedChannels()
    {
        _logger.LogInformation("Checking added channels at: {time}", DateTimeOffset.Now);

        // Get all channels from the database\
        var channels = _databaseHelper.GetDayOldCheckedChannels();
        if (channels.Count == 0)
        {
            _logger.LogInformation("No channels to check");
            return;
        }
        _logger.LogInformation("Channels to check: {0}", channels.Count);
        foreach (var channel in channels)
        {
            bool alreadyAddedVideo = false;
            var channelID = channel.ChannelID;
            var handle = channel.Handle;
            var tags = channel.Tags;
            var lastChecked = channel.LastChecked;

            // Get the newest video for the channel
            var newestVideo = await _youTubeAPI.GetNewestVideoAsync(channelID);
            if (newestVideo.Title == "FAIL")
            {
                _logger.LogWarning("Error fetching newest video for channel: {0}", newestVideo.VideoID);
                continue;
            }
            if (_databaseHelper.IsInVideosTable(newestVideo.VideoID))
            {
                _logger.LogInformation("Video already in database: {0}", newestVideo);
                _databaseHelper.isVideoAddedToPlaylist(newestVideo.VideoID);
                alreadyAddedVideo = _databaseHelper.isVideoAddedToPlaylist(newestVideo.VideoID);
            }
            // Check if the video is already in the playlist

            if (!alreadyAddedVideo)
            {  
                _logger.LogInformation("Video not in database: {0}", newestVideo);
                _databaseHelper.InsertVideo(newestVideo.VideoID, channelID,newestVideo.Title, newestVideo.Description);
                // Check if the video is already in the playlist
                var isInPlaylist = _databaseHelper.IsVideoInPlaylist(newestVideo.VideoID, channelID);
                if (!isInPlaylist)
                {
                    // Add the video to the playlist
                    var playlistID = await  _youTubeAPI.CreatePlaylistAsync("Name", "tag");
                    if (playlistID.Name == "FAIL")
                    {
                        _logger.LogWarning("Error creating playlist for TV channel: {0}", playlistID.PlaylistID);
                        continue;
                    }
                    _databaseHelper.UpdateAddedVideoToPlaylist(newestVideo.VideoID);
                    _databaseHelper.InsertPlaylist(playlistID.PlaylistID, playlistID.Name,playlistID.Description, "Tags");
                    _logger.LogInformation("Video added to playlist: {0}", newestVideo.VideoID);

                }
            }

        }

    }


    public async Task<Channel> GetChannelFromHandle(string handle)
    {
        // Simulate creating a playlist
        _logger.LogInformation("Getting Channel at: {0}", DateTime.UtcNow);
        return await _youTubeAPI.GetChannelFromHandleAsync(handle);

    }

    public async Task<YTvideo> GetNewestVideoAsync(string channelID)
    {
        // Simulate getting the newest video
        _logger.LogInformation("Getting newest video at: {0}", DateTime.UtcNow);
        return await _youTubeAPI.GetNewestVideoAsync(channelID);
    }

    public async Task<YTvideo> GetHandleLastestVideo(string handle)
    {
        var results = await GetChannelFromHandle(handle);
        var channelID = results.ChannelID;
        var resultsVideo = await GetNewestVideoAsync(channelID);
        return resultsVideo;
    }

    public async Task<Services.Playlist> CreatePlaylist(string title, string description)
    {
        // Simulate creating a playlist
        _logger.LogInformation("Creating playlist at: {0}", DateTime.UtcNow);
        return await _youTubeAPI.CreatePlaylistAsync(title, description);

    }


}