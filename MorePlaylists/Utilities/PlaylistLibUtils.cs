using MorePlaylists.Entries;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MorePlaylists.Utilities
{
    public class PlaylistLibUtils
    {
        internal static void SavePlaylist(IGenericEntry playlistEntry)
        {
            BeatSaberPlaylistsLib.Types.IPlaylist playlist = playlistEntry.RemotePlaylist;
            BeatSaberPlaylistsLib.PlaylistManager playlistManager = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.CreateChildManager(playlistEntry.GetType().Name.Replace("Entry", ""));

            // Generate Name
            string playlistFolderPath = playlistManager.PlaylistPath;
            string playlistFileName = string.Join("_", playlist.Title.Replace("/", "").Replace("\\", "").Replace(".", "").Replace(":", "").Replace("*", "").Replace("?", "")
                .Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "").Split());
            if (string.IsNullOrEmpty(playlistFileName))
            {
                playlistFileName = "playlist";
            }
            string extension = playlistManager.DefaultHandler?.DefaultExtension;
            string playlistPath = Path.Combine(playlistFolderPath, playlistFileName + "." + extension);
            string originalPlaylistPath = Path.Combine(playlistFolderPath, playlistFileName);
            int dupNum = 0;
            while (File.Exists(playlistPath))
            {
                dupNum++;
                playlistPath = originalPlaylistPath + string.Format("({0}).{1}", dupNum, extension);
            }
            if (dupNum != 0)
            {
                playlistFileName += string.Format("({0})", dupNum);
            }
            playlist.Filename = playlistFileName;

            playlistManager.StorePlaylist(playlist);
        }

        internal static void DeletePlaylistIfExists(IGenericEntry playlistEntry)
        {
            BeatSaberPlaylistsLib.Types.IPlaylist playlist = playlistEntry.RemotePlaylist;
            if (playlist != null)
            {
                BeatSaberPlaylistsLib.PlaylistManager playlistManager = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.CreateChildManager(playlistEntry.GetType().Name.Replace("Entry", ""));
                if (playlistManager.GetAllPlaylists(false).Contains(playlist))
                {
                    playlistManager.DeletePlaylist(playlist);
                }
            }
        }

        internal static void UpdatePlaylistsOwned(List<IGenericEntry> playlistEntries)
        {
            List<BeatSaberPlaylistsLib.Types.IPlaylist> playlists = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetAllPlaylists(true).ToList();
            Dictionary<string, BeatSaberPlaylistsLib.Types.IPlaylist> syncURLs = new Dictionary<string, BeatSaberPlaylistsLib.Types.IPlaylist>();
            foreach (var playlist in playlists)
            {
                if (playlist.TryGetCustomData("syncURL", out object url))
                {
                    if (url is string urlString)
                    {
                        syncURLs.Add(urlString, playlist);
                    }
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
