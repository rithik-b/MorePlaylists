using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using SiraUtil.Web;

namespace MorePlaylists.Entries
{
    public interface IEntry
    {
        string Title { get; }
        string Author { get; }
        string Description { get; }
        string PlaylistURL { get; }
        string SpriteURL { get; }
        IPlaylist? LocalPlaylist { get; set; }
        bool DownloadBlocked { get; set; }
        bool ExhaustedPages { get; }
        Task<List<Song>?> GetSongs(IHttpService siraHttpService, CancellationToken cancellationToken = default, bool firstPage = false);
        Task<IPlaylist?> DownloadPlaylist(IHttpService siraHttpService, CancellationToken cancellationToken = default);
    }
}
