using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using BeatSaverSharp.Models.Pages;
using MorePlaylists.Entries;
using SiraUtil.Web;
using Playlist = BeatSaverSharp.Models.Playlist;

namespace MorePlaylists.BeatSaver;

public class BeatSaverEntry : IEntry
{
    private readonly Playlist playlist;
    private PlaylistDetail? playlistDetail;

    public BeatSaverEntry(Playlist playlist)
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
    
    public async Task<List<Song>?> GetSongs(IHttpService siraHttpService, CancellationToken cancellationToken = default)
    { 
        playlistDetail = await playlist.GetPlaylistDetail(cancellationToken);

        if (playlistDetail == null)
        {
            return null;
        }

        var songs = new List<Song>();
        
        while (playlistDetail is {Empty: false})
        {
            foreach (var beatmap in playlistDetail.Beatmaps)
            {
                songs.Add(new Song(beatmap.Map.Name, $"{beatmap.Map.Metadata.SongAuthorName} [{beatmap.Map.Metadata.LevelAuthorName}]", beatmap.Map.LatestVersion.CoverURL));
            }
            playlistDetail = await playlistDetail.Next(cancellationToken);
        }

        return songs;
    }
}
