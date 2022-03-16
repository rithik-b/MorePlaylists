using MorePlaylists.Entries;
using System.Collections.Generic;
using System.Linq;

namespace MorePlaylists.Utilities
{
    public class PlaylistLibUtils
    {
        internal static BeatSaberPlaylistsLib.Types.IPlaylist SavePlaylist(IGenericEntry playlistEntry)
        {
            var playlist = playlistEntry.RemotePlaylist;
            var playlistManager = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.CreateChildManager(playlistEntry.GetType().Name.Replace("Entry", ""));

            playlistManager.StorePlaylist(playlist);
            playlistEntry.LocalPlaylist = playlist;
            return playlist;
        }

        internal static void UpdatePlaylistsOwned(List<IGenericEntry> playlistEntries)
        {
            var playlists = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetAllPlaylists(true).ToList();
            var syncURLs = new Dictionary<string, BeatSaberPlaylistsLib.Types.IPlaylist>();
            foreach (var playlist in playlists)
            {
                if (playlist.TryGetCustomData("syncURL", out var url) && url is string urlString)
                {
                    syncURLs[urlString] = playlist;
                }
            }

            foreach (var playlistEntry in playlistEntries)
            {
                if (syncURLs.TryGetValue(playlistEntry.PlaylistURL, out var playlist))
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
