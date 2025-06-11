using YT_APP.Services;
using YT_APP.Database;
using YT_APP.ServiceStructs;
using Google.Apis.Auth.OAuth2;
using System.IO.Pipes;
using System.Text;
using System.Dynamic;
using System.Text.Json;
namespace YT_APP;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly CustomYouTubeService _youTubeService;
    private readonly DatabaseHelper _databaseHelper;
    private NamedPipeClientStream _pipeClient;


    public Worker(ILogger<Worker> logger, CustomYouTubeService youTubeService, DatabaseHelper databaseHelper)
    {
        _youTubeService = youTubeService;
        _logger = logger;
        _databaseHelper = databaseHelper;
        _pipeClient = new NamedPipeClientStream(".", "YTAppPipeClient", PipeDirection.InOut, PipeOptions.Asynchronous);
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
    }


    private async Task<bool> TryConnectPipeAsync()
    {
        if (_pipeClient.IsConnected) return true;

        try
        {
            _logger.LogInformation("Attempting to connect to pipe...");
            await _pipeClient.ConnectAsync(1000); // 5 seconds timeout
            _logger.LogInformation("Connected to pipe successfully.");
            return true;
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Failed to connect to pipe within the timeout period.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to connect to the pipe.");
            return false;
        }


    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //check if pipe is connected
            if (await TryConnectPipeAsync())
            {

                List<Channel> response;
                _logger.LogInformation("Pipe is connected, waiting for commands...");

                var buffer = new byte[1024];
                int bytesRead = await _pipeClient.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                
                Command commandReq = DeserializeData(buffer);
                _logger.LogInformation("Received command: {commandName} with payload: {payload}", commandReq.CommandName, commandReq.Payload);

                if (commandReq.CommandName == "GetChannels")
                {
                    response = _databaseHelper.GetAllChannels();
                    Command responseCommand = new Command
                    {
                        CommandName = "GetChannelsResponse",
                        Payload = JsonSerializer.Serialize(response, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        })
                    };
                    var responseEncoded = SerializeData(responseCommand);
                    await _pipeClient.WriteAsync(responseEncoded, 0, responseEncoded.Length, stoppingToken);
                    await _pipeClient.FlushAsync(stoppingToken);
                    _logger.LogInformation("Sent response to pipe: {response}", response);
                }
            }
            else
            {
                _logger.LogInformation("Pipe not connected, continuing normal operation.");


                //if pipe is not connected keep running normally 

                _logger.LogInformation("Fetched videos at: {time}", DateTimeOffset.Now);

                //var dayoldChannels = _databaseHelper.GetDayOldCheckedChannels();
                var result = await _youTubeService.AddCheckedChannelLatestVideos();
                if (result == "SUCCESS")
                {
                    _logger.LogInformation("Successfully added checked channels latest videos.");
                }
                else
                {
                    _logger.LogError("Failed to add checked channels latest videos: {error}", result);
                }
                await Task.Delay(20000, stoppingToken);
            }
        }
    }


    private void AddChannelCommand()
    {

    }
    private byte[] SerializeData<T>(T data)
    {
        var serData = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return Encoding.UTF8.GetBytes(serData);

    }

    private Command DeserializeData(byte[] data)
    {
        var jsonString = Encoding.UTF8.GetString(data);
        var jsonStringTrimmed = jsonString.TrimEnd('\0'); // Remove any trailing null characters
        return JsonSerializer.Deserialize<Command>(jsonStringTrimmed, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    
    }
    public struct Command
    {
        public string CommandName { get; set; }
        public string Payload { get; set; }
    }




}
