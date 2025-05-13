using System.Security.Cryptography;
using Xunit;
using YT_APP;
using YT_APP.Database;
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
        var connectionString = configuration.GetSection("DefaultConnection").Value;
            Assert.NotNull(apikey);
            Assert.NotEmpty(apikey);
            Assert.NotNull(applicationName);
            Assert.NotEmpty(applicationName);
            Assert.NotNull(clientId);
            Assert.NotEmpty(clientId);
            Assert.NotNull(testAPIKey);
            Assert.NotEmpty(testAPIKey);
            Assert.NotNull(connectionString);
            Assert.NotEmpty(connectionString);
        
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<DatabaseHelper>>();
        var databaseHelper = new DatabaseHelper(connectionString,logger);
        databaseHelper.CreateDatabase();
        services.AddSingleton<DatabaseHelper>(databaseHelper);

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
            var APIlogger = provider.GetRequiredService<ILogger<IYouTubeAPI>>();
            var Servicelogger = provider.GetRequiredService<ILogger<CustomYouTubeService>>();
            var youTubeAPI = new YouTubeAPIService(APIlogger, testAPIKey,applicationName, clientId);
            var databaseHelper = provider.GetRequiredService<DatabaseHelper>();
            return new CustomYouTubeService(Servicelogger, youTubeAPI, databaseHelper);
        });
        services.AddHostedService<Worker>();



    })
    .Build();

host.Run();
