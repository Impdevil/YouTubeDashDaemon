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


        /// <summary>
        /// Test for GetChannelFromURL method
        /// This test checks if the method correctly retrieves the channel ID from the URL.
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Catagory", "independent")]
        public async Task test_GetChannelFromVanityURL_EXPLODER()
        {
            // Given
            var vanityURL = "ThePrimeTimeagen";
            var channelID = "UCUyeluBRhGPCW4rPe_UvBZQ"; //  @ThePrimeTimeagen
            var Logger = new Mock<ILogger<YouTubeAPIService>>();
            var youtubeAPI = new YouTubeAPIService(Logger.Object, "apikey", "YT_APP", "YT_APP");
            var youtubeService = new CustomYouTubeService(_youTubeServiceLogger, youtubeAPI, databaseHelper);

            // When
            var result = await youtubeService.GetChannelFromHandle(vanityURL);
            // Then
            Assert.Equal(channelID, result.ChannelID);
            Assert.Equal("ThePrimeTime", result.Handle);

        }

        [Fact]
        [Trait("Catagory", "independent")]
        public async Task test_GetLatestVideoFromVanityURL_EXPLODER()
        {
            // Given
            var vanityURL = "ThePrimeTimeagen";
            var channelID = "UCUyeluBRhGPCW4rPe_UvBZQ"; //  @ThePrimeTimeagen
            var newestVideoTitle = "Peak Performance";

            var Logger = new Mock<ILogger<YouTubeAPIService>>();
            var youtubeAPI = new YouTubeAPIService(Logger.Object, "apikey", "YT_APP", "YT_APP");
            var youtubeService = new CustomYouTubeService(_youTubeServiceLogger, youtubeAPI, databaseHelper);

            // When
            var resultHandle = await youtubeService.GetHandleLastestVideo(vanityURL);
            // Then
            Assert.Equal(channelID, resultHandle.ChannelID);


            var resultVideo = await youtubeService.GetNewestVideoAsync(resultHandle.ChannelID);
            Assert.Equal(newestVideoTitle, resultVideo.Title);
        }




        #endregion
        #region Database tests
        [Fact]
        [Trait("Category", "Database")]
        public void test_DatabaseConnection()
        {
            // Given
            var connectionString = "Data Source=YT_APP.db";
            var dbHelper = new DatabaseHelper(connectionString, _DBlogger);

            // When
            dbHelper.CreateDatabase();

            // Then
            Assert.True(File.Exists(connectionString.Replace("Data Source=", "")));
        }
        [Fact]
        [Trait("Category", "Database")]
        public void test_DatabaseInsertChannel()
        {
            // Given
            var connectionString = "Data Source=YT_APP.db";
            var dbHelper = new DatabaseHelper(connectionString, _DBlogger);
            dbHelper.CreateDatabase();

            // When
            dbHelper.InsertChannel("ThePrimeagen", "1234567890", "test, tags");

            // Then
            var channel = dbHelper.GetChannelByID("1234567890");
            Assert.Equal("ThePrimeagen", channel.Handle);
            Assert.Equal("1234567890", channel.ChannelID);


            Test_CreatePlaylistAndAddNewVideosToPlaylist();
        }

        void Test_CreatePlaylistAndAddNewVideosToPlaylist()
        {
            // Given
            var connectionString = "Data Source=YT_APP.db";
            var dbHelper = new DatabaseHelper(connectionString, _DBlogger);
            dbHelper.CreateDatabase();

            // When
            dbHelper.InsertVideo("1224567890", "1234567890", "This is a test video", "this is a test description","00:01:00");
            dbHelper.InsertPlaylist("TP1234567890", "Test Playlist", "This is a test playlist", "test, tags");
            dbHelper.InsertPlaylistVideo("TP1234567890", "1224567890");

            // Then
            var playlist = dbHelper.getPlaylistbyID("TP1234567890");
            Assert.Equal("Test Playlist", playlist.Name);
            Assert.Equal("This is a test playlist", playlist.Description);
        }




        #endregion

        #region exploder + db tests
        [Fact]
        [Trait("Category", "exploder+db")]
        public async Task test_GetChannelIDFromHandleAndInsertIntoDB()
        {
            // Given
            var handle = "TheVimeagen";
            var channelID = "UCVk4b-svNJoeytrrlOixebQ";
            var channel = new Services.Channel
            {
                ChannelID = channelID,
                Handle = handle,
                Tags = "test, tags"
            };


            var Logger = new Mock<ILogger<YouTubeAPIService>>();
            var youtubeAPI = new YouTubeAPIService(Logger.Object, "apikey", "YT_APP", "YT_APP");
            var youtubeService = new CustomYouTubeService(_youTubeServiceLogger, youtubeAPI, databaseHelper);
            var result = await youtubeService.AddChannelToCheckList(handle, channel.Tags);

            // When


            // Then
            var dbChannel = databaseHelper.GetChannelByID(channelID);
            Assert.Equal(handle, dbChannel.Handle);
            Assert.Equal("SUCCESS",result);
        }
        #endregion

        #region GoogleAPI tests


        #endregion
    }
}
