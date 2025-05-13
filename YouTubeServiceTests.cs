using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.IO;
using YT_APP.Services;
using YT_APP.Database;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace YT_APP.Tests
{
    public class YouTubeServiceTests
    {
        private readonly Mock<ILogger<IYouTubeAPI>> _APIlogger;

        private readonly ILogger<DatabaseHelper> _DBlogger;
        private readonly ILogger<CustomYouTubeService> _youTubeServiceLogger;
        private readonly Mock<IYouTubeAPI> _youTubeAPIMock;
        private readonly CustomYouTubeService _youTubeService;
        private readonly DatabaseHelper databaseHelper;
        private readonly string _connectionString = "Data Source=YT_APP.db";

        public YouTubeServiceTests()
        {
            // if (File.Exists(_connectionString.Replace("Data Source=", "")))
            // {
            //     File.Delete(_connectionString.Replace("Data Source=", ""));
            // }
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });




            _DBlogger = loggerFactory.CreateLogger<DatabaseHelper>();
            _youTubeServiceLogger = loggerFactory.CreateLogger<CustomYouTubeService>();
            _APIlogger = new Mock<ILogger<IYouTubeAPI>>();
            //_APIlogger = loggerFactory.CreateLogger<IYouTubeAPI>();
            // _DBlogger = new Mock<ILogger<IYouTubeAPI>>();

            _youTubeAPIMock = new Mock<IYouTubeAPI>();

            // databaseHelper = new DatabaseHelper("Data Source=YT_APP.db");
            // databaseHelper.RemoveDatabase();
            databaseHelper = new DatabaseHelper(_connectionString, _DBlogger);
            databaseHelper.CreateDatabase();


            _youTubeService = new CustomYouTubeService(_youTubeServiceLogger, _youTubeAPIMock.Object, databaseHelper);
        }


        #region independent tests
        /// <summary>
        /// Test for GetNewestVideoAsync method 
        /// This test checks if the method correctly retrieves the newest video ID for a given channel ID.
        /// </summary>
        [Fact]
        [Trait("Category", "independent")]
        public async Task test_GetNewestVideoAsync()
        {

            var channelID = "1234567890";
            var newestVideo = new YTvideo
            {
                VideoID = "1234567890",
                ChannelID = channelID,
                Title = "Test Video",
                Description = "This is a test video",
                PublishedAt = DateTime.Now
            };

            _youTubeAPIMock.Setup(x => x.GetNewestVideoAsync(channelID)).Returns(Task.FromResult(newestVideo));
            var result = await _youTubeService.GetNewestVideoAsync(channelID);

            Assert.Equal(newestVideo, result);

            _youTubeAPIMock.Verify(x => x.GetNewestVideoAsync(channelID), Times.Once);
            return;
        }

        /// <summary>
        /// Test for GetChannelIDFromHandleAsync method
        /// This test checks if the method correctly retrieves the channel ID from the handle.
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "independent")]
        public async Task test_GetChannelIDFromHandleAsync()
        {
            var handle = "ThePrimeagen";
            var channel = new Services.Channel
            {
                ChannelID = "1234567890",
                Handle = handle,
                Tags = "test, tags",

            };

            _youTubeAPIMock.Setup(x => x.GetChannelFromHandleAsync(handle)).Returns(Task.FromResult(channel));
            var result = await _youTubeService.GetChannelFromHandle(handle);

            Assert.Equal(channel, result);

            _youTubeAPIMock.Verify(x => x.GetChannelFromHandleAsync(handle), Times.Once);
        }

        [Fact]
        [Trait("Category", "independent")]
        public async Task test_GetHandleLastestVideo()
        {
            var handle = "ThePrimeagen";
            var channel = new Services.Channel
            {
                ChannelID = "1234567890",
                Handle = handle,
                Tags = "test, tags"
            };
            var newestVideo = new Services.YTvideo
            {
                VideoID = "TDD: The Good, The Bad, and The Trash (The Standup)",
                ChannelID = channel.ChannelID,
                Title = "Test Video",
                Description = "This is a test video",
                PublishedAt = DateTime.Now
            };

            _youTubeAPIMock.Setup(x => x.GetChannelFromHandleAsync(handle)).Returns(Task.FromResult(channel));
            _youTubeAPIMock.Setup(x => x.GetNewestVideoAsync(channel.ChannelID)).Returns(Task.FromResult(newestVideo));

            var result = await _youTubeService.GetHandleLastestVideo(handle);

            Assert.Equal(newestVideo, result);

            _youTubeAPIMock.Verify(x => x.GetChannelFromHandleAsync(handle), Times.Once);
            _youTubeAPIMock.Verify(x => x.GetNewestVideoAsync(channel.ChannelID), Times.Once);
        }


        [Fact]
        [Trait("Category", "independent")]
        public async Task test_AddNewChannelToCheckAsync()
        {
            // Given
            var handle = "ThePrimeagen";
            Services.Channel channel = new Services.Channel
            {
                ChannelID = "1234567890",
                Handle = handle,
                Tags = "test, tags",
            };
            _youTubeAPIMock.Setup(x => x.GetChannelFromHandleAsync(handle)).Returns(Task.FromResult(channel));

            // When
            var result = await _youTubeService.AddNewChannelToCheck(handle, "tech,tags");

            // Then
            Assert.Equal(channel.ChannelID, result.ToString());
            _youTubeAPIMock.Verify(x => x.GetChannelFromHandleAsync(handle), Times.Once);
        }


        [Fact]
        [Trait("Category", "independent")]
        public async Task test_CheckLatestChannelsAsync()
        {

            // Given
            var handle = "ThePrimeTimeagen";

            var channel = new Services.Channel
            {
                ChannelID = "abcdefghijklmnop",
                Handle = handle,
                Tags = "test, tech, tags",
            };
            databaseHelper.InsertChannel(handle: channel.Handle, channelId: channel.ChannelID, tags: channel.Tags);            

            var newestVideo = new Services.YTvideo
            {
                VideoID = "TDD: The Good, The Bad, and The Trash (The Standup)",
                ChannelID = channel.ChannelID,
                Title = "Test Video",
                Description = "This is a test video",
                PublishedAt = DateTime.Now
            };
            var newPlayList = new Services.Playlist
            {
                PlaylistID = "1234567890",
                Name = "Test Playlist",
                Description = "This is a test playlist",
                CreatedAt = DateTime.Now
            };

            _youTubeAPIMock.Setup(x => x.GetNewestVideoAsync(channel.ChannelID)).Returns(Task.FromResult(newestVideo));
            _youTubeAPIMock.Setup(x => x.CreatePlaylistAsync("Name","tag")).Returns(Task.FromResult(newPlayList));



            // When
            await _youTubeService.CheckAddedChannels();


            // Then

            _youTubeAPIMock.Verify(x => x.GetNewestVideoAsync(channel.ChannelID), Times.Once);

            Database.Channel results = databaseHelper.GetChannelByID(channel.ChannelID);
            Assert.Equal(channel.Handle, results.Handle);
            Assert.Equal(channel.ChannelID, results.ChannelID);
            Assert.Equal(channel.Tags, results.Tags);
            //Assert.Equal(DateTime.UtcNow, results.LastChecked);
            //never check dates that go to the millisecond

        }




        // [Fact]
        // [Trait("Category", "independent")]
        // public async Task CreatePlaylistAsync()
        // {
        //     Console.WriteLine("CreatePlaylistAsync:");
        //     Console.WriteLine("_________________________________________________________________");
        //     var title = "Test Playlist";
        //     var description = "This is a test playlist";
        //     var playlistID = "1234567890";

        //     _youTubeAPIMock.Setup(x => x.CreatePlaylistAsync(title, description)).Returns(Task.FromResult(playlistID));
        //     var result = await _youTubeService.CreatePlaylistAsync(title, description);

        //     Assert.Equal(playlistID, result);

        //     _youTubeAPIMock.Verify(x => x.CreatePlaylistAsync(title, description), Times.Once);
        // }

        #endregion

        #region GoogleAPI tests

        [Fact]
        [Trait("Category", "GoogleAPI")]
        public async Task test_GetChannelIDFromHandleAsync_GOOGLEAPI_My_Channel_ID()
        {
            var handle = "tartankavujr";
            var channelID = "UC6-9JgSYZt7Q_tS2KIWMwFg"; //mine
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var apikey = config.GetSection("Keys:TESTAPIKEY").Value;
            Assert.NotNull(apikey);
            Assert.NotEmpty(apikey);
            var Logger = new Mock<ILogger<YouTubeAPIService>>();
            var youtubeAPI = new YouTubeAPIService(Logger.Object, apikey, "YT_APP", "YT_APP");
            var youtubeService = new CustomYouTubeService(_youTubeServiceLogger, youtubeAPI, databaseHelper);


            // Act
            var result = await youtubeService.GetChannelFromHandle(handle);

            // Assert
            Assert.Equal(channelID, result.ChannelID);
        }

        [Fact]
        [Trait("Category", "GoogleAPI")]
        public async Task test_GetChannelIDFromHandleAsync_GOOGLEAPI_2()
        {
            var handle = "@ThePrimeTimeagen";
            var channelID = "UCUyeluBRhGPCW4rPe_UvBZQ"; //  @ThePrimeTimeagen
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var apikey = config.GetSection("Keys:TESTAPIKEY").Value;
            Assert.NotNull(apikey);
            Assert.NotEmpty(apikey);
            var Logger = new Mock<ILogger<YouTubeAPIService>>();
            var youtubeAPI = new YouTubeAPIService(Logger.Object, apikey, "YT_APP", "YT_APP");
            var youtubeService = new CustomYouTubeService(_youTubeServiceLogger, youtubeAPI, databaseHelper);

            // Act
            var result = await youtubeService.GetChannelFromHandle(handle);
            // Assert
            Assert.Equal(channelID, result.ChannelID);
        }


        [Fact]
        [Trait("Category", "GoogleAPI")]
        public async Task test_GetHandleLastestVideo_GOOGLEAPI_thePrime()
        {
            var handle = "@ThePrimeTimeagen";

            var channelID = "UCUyeluBRhGPCW4rPe_UvBZQ"; //  @ThePrimeTimeagen
            var newestVideoID = "TDD: The Good, The Bad, and The Trash (The Standup)";

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var apikey = config.GetSection("Keys:TESTAPIKEY").Value;
            Assert.NotNull(apikey);
            Assert.NotEmpty(apikey);
            var Logger = new Mock<ILogger<YouTubeAPIService>>();
            var youtubeAPI = new YouTubeAPIService(Logger.Object, apikey, "YT_APP", "YT_APP");
            var youtubeService = new CustomYouTubeService(_youTubeServiceLogger, youtubeAPI, databaseHelper);

            // Act
            var resultHandle = await youtubeService.GetChannelFromHandle(handle);
            // Assert

            var resultVideo = await _youTubeService.GetHandleLastestVideo(handle);

            Console.WriteLine("newestVideoID: {0}", newestVideoID);
            Console.WriteLine("Result: {0}", resultVideo);
            Assert.Equal(channelID, resultHandle.ChannelID);
            Assert.Equal(newestVideoID, resultVideo.VideoID);


        }


        #endregion
    }
}
