using Microsoft.Data.Sqlite;


namespace YT_APP.Database
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
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
                        channel_id TEXT NOT NULL,
                        handle TEXT NOT NULL,
                        tag TEXT NOT NULL,
                        created_at TEXT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS videos (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        video_id TEXT NOT NULL,
                        channel_id TEXT NOT NULL,
                        title TEXT NOT NULL,
                        description TEXT NOT NULL,
                        published_at TEXT NOT NULL,
                        FOREIGN KEY (channel_id) REFERENCES channels (channel_id)
                    );
                    CREATE TABLE IF NOT EXISTS playlists (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        playlist_id TEXT NOT NULL,
                        name TEXT NOT NULL,
                        description TEXT,
                        created_at TEXT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS playlist_videos (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        playlist_id TEXT NOT NULL,
                        video_id TEXT NOT NULL,
                        added_at TEXT NOT NULL,
                        FOREIGN KEY (playlist_id) REFERENCES playlists (playlist_id),
                        FOREIGN KEY (video_id) REFERENCES videos (video_id)
                    );
                ";

                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void InsertChannel(string channelId, string handle, string tag)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO channels (channel_id, handle, tag, created_at)
                    VALUES ($channel_id, $handle, $tag, $created_at);
                ";
                command.Parameters.AddWithValue("$channel_id", channelId);
                command.Parameters.AddWithValue("$handle", handle);
                command.Parameters.AddWithValue("$tag", tag);
                command.Parameters.AddWithValue("$created_at", DateTime.UtcNow.ToString("o"));
                command.ExecuteNonQuery();
            }
        }
        public void InsertVideo(string videoId, string channelId, string title, string description)
        {
            using (var connection = GetConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO videos (video_id, channel_id, title, description, published_at)
                    VALUES ($video_id, $channel_id, $title, $description, $published_at);
                ";
                command.Parameters.AddWithValue("$video_id", videoId);
                command.Parameters.AddWithValue("$channel_id", channelId);
                command.Parameters.AddWithValue("$title", title);
                command.Parameters.AddWithValue("$description", description);
                command.Parameters.AddWithValue("$published_at", DateTime.UtcNow.ToString("o"));
                command.ExecuteNonQuery();
            }
        }

        
    }
}