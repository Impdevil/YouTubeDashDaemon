using YT_APP.Services;

namespace YT_APP;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly CustomYouTubeService _youTubeService;


    public Worker(ILogger<Worker> logger,CustomYouTubeService youTubeService)
    {
        _youTubeService = youTubeService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //var channelID = "UCUyeluBRhGPCW4rPe_UvBZQ";
            // Simulate some work
            //var result = await _youTubeService.GetNewestVideoAsync(channelID);
            _logger.LogInformation("Fetched videos at: {time}", DateTimeOffset.Now);
            //_logger.LogInformation("Newest Video ID: {result}", result);
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(20000, stoppingToken);
        }
    }
}
