using System.Threading.Channels;
using Google;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
namespace YT_APP.Services;


public interface IYouTubeAPI
{
    Task<string> GetChannelIDFromHandleAsync(string handle);
    Task<string> GetNewestVideoAsync(string channelID);
    Task<string> CreatePlaylistAsync(string title, string description);
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


    public async Task<string> GetChannelIDFromHandleAsync(string handle)
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
                return "Error fetching channel ID";
            }
            var channel = channelsListResponse.Items.FirstOrDefault();
            if (channel != null)
            {
                return channel.Id;
            }
            else
            {
                _logger.LogWarning("No channel found for handle: {handle}", handle);
                return "Error fetching channel ID";
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


    public async Task<string> GetNewestVideoAsync(string channelID)
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
            return newestVideo.ToString();
        }
        else
        {
            _logger.LogWarning("No videos found for channel ID: {channelID}", channelID);
            return "Error fetching video";
        }
    }
    public async Task<string> CreatePlaylistAsync(string title, string description)
    {
        // Simulate creating a playlist
        _logger.LogInformation("Creating playlist at: {time}", DateTimeOffset.Now);


        throw new NotImplementedException("CreatePlaylistAsync method is not implemented yet.");
        //return Task.FromResult("Playlist ID");

        return "Playlist ID";
    }

}