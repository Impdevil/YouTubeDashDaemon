namespace YT_APP.ServiceStructs;

public struct Video
{
    public string VideoID { get; set; }
    public string ChannelID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool AddedToPlaylist { get; set; }
    public string Duration { get; set; }
    public DateTime PublishedAt { get; set; }
}
public struct Channel
{
    public string ChannelID { get; set; }
    public string Handle { get; set; }
    public string Tags { get; set; }
    public DateTime LastChecked { get; set; }
}

public struct Playlist
{
    public string PlaylistID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Tags { get; set; }
    public DateTime CreatedAt { get; set; }
}
public struct PlaylistVideo
{
    public string PlaylistID { get; set; }
    public string VideoID { get; set; }
    public DateTime AddedAt { get; set; }
}
