namespace YT_APP.Pipeline;

using System.IO.Pipes;
using System.IO;
using System.Text.Json;
using System.Text;



public struct Command
{
    public string CommandName { get; set; }
    public string Payload { get; set; }
}



class CommandPipelineConnector{
    private NamedPipeClientStream _pipeClient;
    ILogger<CommandPipelineConnector> _logger; 

    public CommandPipelineConnector(){
        _pipeClient = new NamedPipeClientStream(".", "YTAppPipeClient", PipeDirection.InOut, PipeOptions.Asynchronous);

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _logger = loggerFactory.CreateLogger<CommandPipelineConnector>();;    

    }



    public async Task<bool> TryConnectPipeAsync()
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

    public async Task<Command> ReadInboundCommand(CancellationToken stoppingToken){
        var buffer = new byte[1024];
        int bytesRead = await _pipeClient.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                
        Command commandReq = DeserializeData(buffer);
        _logger.LogInformation("Received command: {commandName} with payload: {payload}", commandReq.CommandName, commandReq.Payload);

        return commandReq;
    }
    public async Task RespondCommand<T>(string responseType, T response, CancellationToken stoppingToken){
        
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
    }

}
