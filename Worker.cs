using YT_APP.Services;
using YT_APP.Database;
using YT_APP.ServiceStructs;
using Google.Apis.Auth.OAuth2;
using System.Text;
using System.Dynamic;
using YT_APP.Pipeline;
namespace YT_APP;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly CustomYouTubeService _youTubeService;
    private readonly DatabaseHelper _databaseHelper;
    private CommandPipelineConnector _commPipeConnector;



    public Worker(ILogger<Worker> logger, CustomYouTubeService youTubeService, DatabaseHelper databaseHelper)
    {
        _youTubeService = youTubeService;
        _logger = logger;
        _databaseHelper = databaseHelper;
        _commPipeConnector = new CommandPipelineConnector();
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
    }




    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //check if pipe is connected
            if (await _commPipeConnector.TryConnectPipeAsync())
            {
                //

                _logger.LogInformation("Pipe is connected, waiting for commands...");
                Command commandReq = await _commPipeConnector.ReadInboundCommand(stoppingToken);
                
                if (commandReq.CommandName == "GetChannels")
                {
                    var response = _databaseHelper.GetAllChannels();
                    await _commPipeConnector.RespondCommand("GetChannel",response,stoppingToken);
                }
                DoQuery(commandReq, stoppingToken);

                
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
    


    public void DoQuery(Command command,CancellationToken stoppingToken){
        switch (command.CommandName)
        {
            case "GetChannels":
                var response = _databaseHelper.GetAllChannels();
                _commPipeConnector.RespondCommand<List<Channel>>("GetChannel",response,stoppingToken);
                break;
            
        }
    }
}
