using MorePlaylists.Entries;
using System.Collections.Generic;
using System.Linq;
using BeatSaberPlaylistsLib.Types;

namespace MorePlaylists.Utilities
{
    internal static class PlaylistLibUtils
    {
        public static void SavePlaylist(IEntry entry, IPlaylist playlist)
        {
            var playlistManager = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.CreateChildManager(entry.FolderName);
            playlistManager.StorePlaylist(playlist);
            entry.LocalPlaylist = playlist;
        }

        public static void UpdatePlaylistsOwned(List<IEntry> entries)
        {
            var playlists = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetAllPlaylists(true).ToList();
            var syncURLs = new Dictionary<string, IPlaylist>();
            
            foreach (var playlist in playlists)
            {
                if (playlist.TryGetCustomData("syncURL", out var url) && url is string urlString)
                {
                    syncURLs[urlString] = playlist;
                }
            }

            foreach (var playlistEntry in entries)
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
