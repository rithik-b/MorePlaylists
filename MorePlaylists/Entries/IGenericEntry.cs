using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiraUtil.Web;

namespace MorePlaylists.Entries
{
    public interface IGenericEntry
    {
        string Title { get; }
        string Author { get; }
        string Description { get; }
        string PlaylistURL { get; }
        string SpriteString { get; }
        SpriteType SpriteType { get; }
        BeatSaberPlaylistsLib.Types.IPlaylist RemotePlaylist { get; }
        BeatSaberPlaylistsLib.Types.IPlaylist LocalPlaylist { get; set; }
        event Action<IGenericEntry> FinishedDownload;
        DownloadState DownloadState { get; set; }
        bool DownloadBlocked { get; set; }
        Task DownloadPlaylist(IHttpService siraHttpService);
        Task<List<Song>> GetSongs(IHttpService siraHttpService);
    }
}
