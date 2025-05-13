using System.Security.Cryptography;
using Xunit;
using YT_APP;
using YT_APP.Services;
using YT_APP.Tests;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var apikey = configuration.GetSection("Keys:YoutubeKey:installed:client_secret").Value;
        var applicationName = configuration.GetSection("Keys:YoutubeKey:installed:project_id").Value;
        var clientId = configuration.GetSection("Keys:YoutubeKey:installed:client_id").Value;
        var testAPIKey = configuration.GetSection("Keys:TESTAPIKEY").Value;
        Console.WriteLine($"API Key: {apikey}");
        services.AddHostedService<Worker>();
        services.AddSingleton<CustomYouTubeService>(provider =>
        {
            Assert.NotNull(apikey);
            Assert.NotEmpty(apikey);
            Assert.NotNull(applicationName);
            Assert.NotEmpty(applicationName);
            Assert.NotNull(clientId);
            Assert.NotEmpty(clientId);
            Assert.NotNull(testAPIKey);
            Assert.NotEmpty(testAPIKey);
            var logger = provider.GetRequiredService<ILogger<IYouTubeAPI>>();
            var youTubeAPI = new YouTubeAPIService(logger, testAPIKey,applicationName, clientId);
            return new CustomYouTubeService(logger, youTubeAPI);
        });


    })
    .Build();

host.Run();
