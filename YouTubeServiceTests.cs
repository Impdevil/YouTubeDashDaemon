using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Moq;
using Xunit;


using YT_APP.Services;

namespace YT_APP.Tests
{
    public class YouTubeServiceTests
    {
        private readonly Mock<ILogger<IYouTubeAPI>> _loggerMock;
        private readonly Mock<IYouTubeAPI> _youTubeAPIMock;
        private readonly CustomYouTubeService _youTubeService;


        public YouTubeServiceTests()
        {
            _loggerMock = new Mock<ILogger<IYouTubeAPI>>();
            _youTubeAPIMock = new Mock<IYouTubeAPI>();
            _youTubeService = new CustomYouTubeService(_loggerMock.Object, _youTubeAPIMock.Object);
        }

        [Fact]
        [Trait("Category", "independent")]
        public async Task GetNewestVideoAsync()
        {
            Console.WriteLine("GetNewestVideoAsync:");
            Console.WriteLine("_________________________________________________________________");

            var channelID = "ThePrimeagenChannel";
            var newestVideoID = "1234567890";

            _youTubeAPIMock.Setup(x => x.GetNewestVideoAsync(channelID)).Returns(Task.FromResult(newestVideoID));
            var result = await _youTubeService.GetNewestVideoAsync(channelID);

            Assert.Equal(newestVideoID, result);

            _youTubeAPIMock.Verify(x => x.GetNewestVideoAsync(channelID), Times.Once);
            return;
        }

        [Fact]
        [Trait("Category", "independent")]
        public async Task GetChannelIDFromHandleAsync()
        {
            Console.WriteLine("GetChannelIDFromHandleAsync:");
            Console.WriteLine("_________________________________________________________________");
            var handle = "ThePrimeagen";
            var channelID = "1234567890";

            _youTubeAPIMock.Setup(x => x.GetChannelIDFromHandleAsync(handle)).Returns(Task.FromResult(channelID));
            var result = await _youTubeService.GetChannelIDFromHandle(handle);

            Assert.Equal(channelID, result);

            _youTubeAPIMock.Verify(x => x.GetChannelIDFromHandleAsync(handle), Times.Once);
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
        [Fact]
        [Trait("Category", "GoogleAPI")]
        public async Task GetChannelIDFromHandleAsync_GOOGLEAPI_My_Channel_ID()
        {
            Console.WriteLine("GetChannelIDFromHandleAsync_GOOGLEAPI:");
            Console.WriteLine("_________________________________________________________________");
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
            var youtubeService = new CustomYouTubeService(Logger.Object, youtubeAPI);


            // Act
            var result = await youtubeService.GetChannelIDFromHandle(handle);

            Console.WriteLine("Handle: {0}", handle);
            Console.WriteLine("Channel ID: {0}", channelID);
            Console.WriteLine("Result: {0}", result);
            // Assert
            Assert.Equal(channelID, result);
        }

        [Fact]
        [Trait("Category", "GoogleAPI")]
        public async Task GetChannelIDFromHandleAsync_GOOGLEAPI_2()
        {
            Console.WriteLine("GetChannelIDFromHandleAsync_GOOGLEAPI_2");
            Console.WriteLine("_________________________________________________________________");
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
            var youtubeService = new CustomYouTubeService(Logger.Object, youtubeAPI);

            // Act
            var result = await youtubeService.GetChannelIDFromHandle(handle);
            Console.WriteLine("Handle: {0}", handle);
            Console.WriteLine("Channel ID: {0}", channelID);
            Console.WriteLine("Result: {0}", result);
            // Assert
            Assert.Equal(channelID, result);
        }

        [Fact]
        [Trait("Category", "independent")]
        public async Task GetHandleLastestVideo()
        {
            Console.WriteLine("GetHandleLastestVideo:");
            Console.WriteLine("_________________________________________________________________");
            var handle = "ThePrimeagen";
            var channelID = "1234567890";
            var newestVideoID = "TDD: The Good, The Bad, and The Trash (The Standup)";

            _youTubeAPIMock.Setup(x => x.GetChannelIDFromHandleAsync(handle)).Returns(Task.FromResult(channelID));
            _youTubeAPIMock.Setup(x => x.GetNewestVideoAsync(channelID)).Returns(Task.FromResult(newestVideoID));

            var result = await _youTubeService.GetHandleLastestVideo(handle);

            Assert.Equal(newestVideoID, result);

            _youTubeAPIMock.Verify(x => x.GetChannelIDFromHandleAsync(handle), Times.Once);
            _youTubeAPIMock.Verify(x => x.GetNewestVideoAsync(channelID), Times.Once);
        }
        [Fact]
        [Trait("Category", "GoogleAPI")]
        public async Task GetHandleLastestVideo_GOOGLEAPI_thePrime()
        {
            Console.WriteLine("GetHandleLastestVideo_GOOGLEAPI_thePrime:");
            Console.WriteLine("_________________________________________________________________");
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
            var youtubeService = new CustomYouTubeService(Logger.Object, youtubeAPI);

            // Act
            var resultHandle = await youtubeService.GetChannelIDFromHandle(handle);
            Console.WriteLine("Handle: {0}", handle);
            Console.WriteLine("Result: {0}", resultHandle);
            // Assert

            var resultVideo = await _youTubeService.GetHandleLastestVideo(handle);

            Console.WriteLine("newestVideoID: {0}", newestVideoID);
            Console.WriteLine("Result: {0}", resultVideo);
            Assert.Equal(channelID, resultHandle);
            Assert.Equal(newestVideoID, resultVideo);


        }

    }
}
