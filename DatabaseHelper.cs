using Microsoft.Data.Sqlite;
using System.Data;
using System.IO;
using YT_APP.Services;
using YT_APP.ServiceStructs;

namespace YT_APP.Database
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseHelper> _logger;

        public DatabaseHelper(string connectionString, ILogger<DatabaseHelper> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            
        }

        public SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public void RemoveDatabase()
        {
            if (File.Exists(_connectionString.Replace("Data Source=", "")))
            {
                File.Delete(_connectionString.Replace("Data Source=", ""));
            }
        }
        public void CreateDatabase()
        {
            using (var connection = GetConnection())
            {
                // Create the database if it doesn't exist
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS channels (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        channel_id TEXT NOT NULL UNIQUE,
                        handle TEXT NOT NULL,
                        tags TEXT NOT NULL,
                        lastChecked TEXT NOT NULL,
                        UploadRate int NOT NULL DEFAULT 1,
                        created_at TEXT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS videos (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        video_id TEXT NOT NULL UNIQUE,
                        channel_id TEXT NOT NULL,
                        title TEXT NOT NULL,
                        description TEXT NOT NULL,
                        duration TEXT NOT NULL,
                        addedtoPlaylist int NOT NULL,
                        published_at TEXT NOT NULL,
                        FOREIGN KEY (channel_id) REFERENCES channels (channel_id)
                    );
                    CREATE TABLE IF NOT EXISTS playlists (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        playlist_id TEXT NOT NULL UNIQUE,
                        playList_tags TEXT,
                        name TEXT NOT NULL,
                        description TEXT,
                        created_at TEXT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS playlist_videos (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        playlist_id TEXT NOT NULL,
                        video_id TEXT NOT NULL,
                        FOREIGN KEY (playlist_id) REFERENCES playlists (playlist_id),
                        FOREIGN KEY (video_id) REFERENCES videos (video_id)
                    );
                ";

                command.ExecuteNonQuery();
                connection.Close();
            }
        }


        #region Insert Methods
        public void InsertChannel(string handle,string channelId, string tags,int uploadRate = 1)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                _logger.LogInformation("Inserting channel: {0} {1} {2} {3} {4}", handle, channelId, tags, DateTime.UtcNow.ToString("o"), DateTime.UtcNow.AddDays(-1).ToString("o"));
                command.CommandText =
                @"
                    INSERT INTO channels (channel_id, handle, tags, created_at, lastChecked, UploadRate)
                    VALUES ($channel_id, $handle, $tags, $created_at,$lastChecked,$UploadRate);
                ";
                command.Parameters.AddWithValue("$channel_id", channelId);
                command.Parameters.AddWithValue("$handle", handle);
                command.Parameters.AddWithValue("$tags", tags);
                command.Parameters.AddWithValue("$created_at", DateTime.UtcNow.ToString("o"));
                command.Parameters.AddWithValue("$UploadRate", uploadRate);
                command.Parameters.AddWithValue("$lastChecked", DateTime.UtcNow.AddDays(-2).ToString("o"));
                command.ExecuteNonQuery();
            }
        }
        
        public void InsertChannel(Channel channel)
        {
            InsertChannel(channel.Handle, channel.ChannelID, channel.Tags);
        }
        public void InsertVideo(string videoId, string channelId, string title, string description, string duration)
        {
            using (var connection = GetConnection())
            {
                _logger.LogInformation("-----Inserting video: {0} ??? {1} ??? {2} ??? {3} ??? {4}???----", videoId, channelId, title, description, duration);
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO videos (video_id, channel_id, title, description, published_at, addedtoPlaylist,duration)
                    VALUES ($video_id, $channel_id, $title, $description, $published_at, 0, $duration);
                ";
                command.Parameters.AddWithValue("$video_id", videoId);
                command.Parameters.AddWithValue("$channel_id", channelId);
                command.Parameters.AddWithValue("$title", title);
                command.Parameters.AddWithValue("$description", description);
                command.Parameters.AddWithValue("$published_at", DateTime.UtcNow.ToString("o"));
                command.Parameters.AddWithValue("$duration", duration);
                command.ExecuteNonQuery();
            }
        }
        public void InsertVideo(Video video)
        {
            InsertVideo(video.VideoID, video.ChannelID, video.Title, video.Description, video.Duration);
        }

        public void InsertPlaylist(string playlistId, string name, string description, string tags)
        {
            _logger.LogInformation("Creating DB Playlist");
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO playlists (playlist_id, name, description, playList_tags , created_at)
                    VALUES ($playlist_id, $name, $description,$tags, $created_at);
                ";
                command.Parameters.AddWithValue("$playlist_id", playlistId);
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$description", description);
                command.Parameters.AddWithValue("$tags", tags);
                command.Parameters.AddWithValue("$created_at", DateTime.UtcNow.ToString("o"));
                command.ExecuteNonQuery();
            }
            _logger.LogInformation("DB created @ {0}", DateTime.UtcNow);
        }

        public void InsertPlaylistVideo(string playlistId, string videoId)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO playlist_videos (playlist_id, video_id)
                    VALUES ($playlist_id, $video_id);
                ";
                command.Parameters.AddWithValue("$playlist_id", playlistId);
                command.Parameters.AddWithValue("$video_id", videoId);
                command.ExecuteNonQuery();
            }
        }
        #endregion

        #region Select Methods
        public List<Channel> GetAllChannels()
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT channel_id, handle, tags,UploadRate FROM channels;
                ";

                using (var reader = command.ExecuteReader())
                {
                    var channels = new List<Channel>();
                    while (reader.Read())
                    {
                        var channel = new Channel
                        {
                            ChannelID = reader.GetString(0),
                            Handle = reader.GetString(1),
                            Tags = reader.GetString(2),
                            UploadRate = reader.GetInt32(3),
                            LastChecked = DateTime.UtcNow
                        };
                        channels.Add(channel);
                    }
                    return channels;
                }
            }
        }

        public List<Channel> GetDayOldCheckedChannels()
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT channel_id, handle, tags, uploadRate FROM channels WHERE lastChecked < datetime('now', '-1 day');
                ";

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return new List<Channel>();
                    }
                    var channels = new List<Channel>();
                    while (reader.Read())
                    {
                        var channel = new Channel
                        {
                            ChannelID = reader.GetString(0),
                            Handle = reader.GetString(1),
                            Tags = reader.GetString(2),
                            UploadRate = reader.GetInt32(3),
                            LastChecked = DateTime.UtcNow
                        };
                        channels.Add(channel);
                    }
                    return channels;
                }
            }
        }

        public Channel GetChannelByID(string channelId)
        {
            using (var connection = GetConnection())
            {
                _logger.LogInformation("Getting channel: {0}", channelId);
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT channel_id, handle, tags, lastChecked FROM channels WHERE channel_id = $channel_id;
                ";
                command.Parameters.AddWithValue("$channel_id", channelId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Channel
                        {
                            ChannelID = reader.GetString(0),
                            Handle = reader.GetString(1),
                            Tags = reader.GetString(2),
                            UploadRate = reader.GetInt32(3),
                            LastChecked = reader.GetDateTime(3)
                        };
                    }
                    else
                    {
                        return new Channel();
                    }
                }
            }
        }

        public List<Video> GetAllVideos()
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT video_id, channel_id, title, description FROM videos;
                ";

                using (var reader = command.ExecuteReader())
                {
                    var videos = new List<Video>();
                    while (reader.Read())
                    {
                        var video = new Video
                        {
                            VideoID = reader.GetString(0),
                            ChannelID = reader.GetString(1),
                            Title = reader.GetString(2),
                            Description = reader.GetString(3),
                            AddedToPlaylist = false,
                            PublishedAt = DateTime.UtcNow
                        };
                        videos.Add(video);
                    }
                    return videos;
                }
            }
        }

        public Playlist getPlaylistbyName(string playlistName)
        {
            _logger.LogInformation("Getting Playlists by name @ {0}", DateTime.UtcNow);
            var playlist = new Playlist();

            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT playlist_id, name, description, playList_tags , created_at FROM playlists
                    WHERE name = $playlistname
                ";
                command.Parameters.AddWithValue("$playlistname", playlistName);

                using (var reader = command.ExecuteReader()){
                     playlist.PlaylistID = reader.GetString(0);
                     playlist.Name = reader.GetString(1);
                     playlist.Description = reader.GetString(2);
                     playlist.Tags = reader.GetString(3);
                     playlist.CreatedAt = DateTime.Parse( reader.GetString(4));
                }
            }
            return playlist;
        }

        public Playlist getPlaylistbyID(string playlistID)
        {
            _logger.LogInformation("Getting Playlists by name @ {0}", DateTime.UtcNow);
            var playlist = new Playlist();

            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT playlist_id, name, description, playList_tags , created_at FROM playlists
                    WHERE playlist_id = $playlist_id
                ";
                command.Parameters.AddWithValue("$playlist_id", playlistID);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        playlist.PlaylistID = reader.GetString(0);
                        playlist.Name = reader.GetString(1);
                        playlist.Description = reader.GetString(2);
                        playlist.Tags = reader.GetString(3);
                        playlist.CreatedAt = DateTime.Parse(reader.GetString(4));
                    }
                }
            }
            return playlist;
        }



        public bool IsInVideosTable(string videoId)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT COUNT(*) FROM videos WHERE video_id = $video_id;
                ";
                command.Parameters.AddWithValue("$video_id", videoId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public bool IsVideoInPlaylist(string videoId, string playlistId)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT COUNT(*) FROM playlist_videos WHERE video_id = $video_id AND playlist_id = $playlist_id;
                ";
                command.Parameters.AddWithValue("$video_id", videoId);
                command.Parameters.AddWithValue("$playlist_id", playlistId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public bool isVideoAddedToPlaylist(string videoId)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT COUNT(*) FROM videos WHERE video_id = $video_id AND addedtoPlaylist = 1;
                ";
                command.Parameters.AddWithValue("$video_id", videoId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }



        #endregion



        #region Update Methods
        public void UpdateChannel(string channelId, string handle, string tags, DateTime lastChecked)
        {
            using (var connection = GetConnection())
            {
                _logger.LogInformation("Updating channel: {0} {1}", channelId, lastChecked);
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE channels SET handle = $handle, tags = $tag, lastChecked = $lastChecked WHERE channel_id = $channel_id;
                ";
                command.Parameters.AddWithValue("$channel_id", channelId);
                command.Parameters.AddWithValue("$handle", handle);
                command.Parameters.AddWithValue("$tag", tags);
                command.Parameters.AddWithValue("$lastChecked", lastChecked);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateChannelLastChecked(ServiceStructs.Channel channel)
        {
            UpdateChannel(channel.ChannelID, channel.Handle, channel.Tags,DateTime.UtcNow);
        }
        public void UpdateVideo(string videoId, string channelId, string title, string description)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE videos SET title = $title, description = $description WHERE video_id = $video_id AND channel_id = $channel_id;
                ";
                command.Parameters.AddWithValue("$video_id", videoId);
                command.Parameters.AddWithValue("$channel_id", channelId);
                command.Parameters.AddWithValue("$title", title);
                command.Parameters.AddWithValue("$description", description);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateAddedVideoToPlaylist(string videoId)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE videos SET addedtoPlaylist = 1 WHERE video_id = $video_id;
                ";
                command.Parameters.AddWithValue("$video_id", videoId);
                command.ExecuteNonQuery();
            }
        }

        #endregion






    }
}