using System.IO;

namespace MorePlaylists.Utilities
{
    public class PlaylistLibUtils
    {
        private static readonly string MANAGER_NAME = "Downloads";

        internal static void SavePlaylist(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            BeatSaberPlaylistsLib.PlaylistManager playlistManager = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.CreateChildManager(MANAGER_NAME);

            // Generate 
            string playlistFolderPath = playlistManager.PlaylistPath;
            string playlistFileName = string.Join("_", playlist.Title.Replace("/", "").Replace("\\", "").Replace(".", "").Split(' '));
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
                playlistFileName = playlistFileName + string.Format("({0})", dupNum);
            }

            playlist.Filename = playlistFileName;
            playlistManager.StorePlaylist(playlist);
        }
    }
}
