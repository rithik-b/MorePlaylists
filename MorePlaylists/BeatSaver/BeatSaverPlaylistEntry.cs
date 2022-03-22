using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using BeatSaverSharp.Models;
using BeatSaverSharp.Models.Pages;
using MorePlaylists.Entries;
using SiraUtil.Web;
using Playlist = BeatSaverSharp.Models.Playlist;

namespace MorePlaylists.BeatSaver;

public class BeatSaverPlaylistEntry : IBeatSaverEntry
{
    private readonly Playlist playlist;
    private PlaylistDetail? playlistDetail;

    public BeatSaverPlaylistEntry(Playlist playlist)
    {
        this.playlist = playlist;
    }

    public string Title => playlist.Name;
    public string Author => playlist.Owner.Name;
    public string Description => playlist.Description;
    public string PlaylistURL => playlist.DownloadURL;
    public string SpriteURL => playlist.CoverURL;
    public IPlaylist? LocalPlaylist { get; set; }
    public bool DownloadBlocked { get; set; }
    public bool ExhaustedPages { get; private set; }
    public User Owner => playlist.Owner;

    public async Task<List<Song>?> GetSongs(IHttpService siraHttpService, CancellationToken cancellationToken = default, bool firstPage = false)
    {
        if (playlistDetail == null || firstPage)
        {
            playlistDetail = await playlist.GetPlaylistDetail(cancellationToken);
            ExhaustedPages = false;
        }
        else
        {
            var newDetail = await playlistDetail.Next(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            playlistDetail = newDetail;
        }

        if (playlistDetail == null || playlistDetail.Empty)
        {
            ExhaustedPages = true;
            return null;
        }

        var songs = new List<Song>();
        foreach (var beatmap in playlistDetail.Beatmaps)
        {
            songs.Add(new Song(beatmap.Map.Name, $"{beatmap.Map.Metadata.SongAuthorName} [{beatmap.Map.Metadata.LevelAuthorName}]", beatmap.Map.LatestVersion.CoverURL));
        }
        return songs;
    }

    public async Task<IPlaylist?> DownloadPlaylist(IHttpService siraHttpService, CancellationToken cancellationToken = default)
    { 
        var playlistBytes = await playlist.DownloadPlaylist(cancellationToken);
        if (playlistBytes != null)
        {
            using var playlistStream = new MemoryStream(playlistBytes);
            return BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(playlistStream);
        }
        return null;
    }
}
