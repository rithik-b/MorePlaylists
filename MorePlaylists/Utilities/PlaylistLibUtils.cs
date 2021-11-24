using MorePlaylists.Entries;
using System.Collections.Generic;
using System.Linq;

namespace MorePlaylists.Utilities
{
    public class PlaylistLibUtils
    {
        internal static BeatSaberPlaylistsLib.Types.IPlaylist SavePlaylist(IGenericEntry playlistEntry)
        {
            BeatSaberPlaylistsLib.Types.IPlaylist playlist = playlistEntry.RemotePlaylist;
            BeatSaberPlaylistsLib.PlaylistManager playlistManager = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.CreateChildManager(playlistEntry.GetType().Name.Replace("Entry", ""));

            playlistManager.StorePlaylist(playlist);
            playlistEntry.LocalPlaylist = playlist;
            return playlist;
        }

        internal static void UpdatePlaylistsOwned(List<IGenericEntry> playlistEntries)
        {
            List<BeatSaberPlaylistsLib.Types.IPlaylist> playlists = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetAllPlaylists(true).ToList();
            Dictionary<string, BeatSaberPlaylistsLib.Types.IPlaylist> syncURLs = new Dictionary<string, BeatSaberPlaylistsLib.Types.IPlaylist>();
            foreach (var playlist in playlists)
            {
                if (playlist.TryGetCustomData("syncURL", out object url) && url is string urlString)
                {
                    syncURLs[urlString] = playlist;
                }
            }

            foreach (var playlistEntry in playlistEntries)
            {
                if (syncURLs.TryGetValue(playlistEntry.PlaylistURL, out BeatSaberPlaylistsLib.Types.IPlaylist playlist))
                {
                    playlistEntry.DownloadBlocked = true;
                    playlistEntry.LocalPlaylist = playlist;
                }
                else
                {
                    playlistEntry.DownloadBlocked = false;
                    playlistEntry.LocalPlaylist = null;
                }
            }
        }
    }
}
