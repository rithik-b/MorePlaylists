using SiraUtil;
using System;
using System.IO;
using System.Threading;

namespace MorePlaylists.Entries
{
    public abstract class GenericEntry : IGenericEntry
    {
        private BeatSaberPlaylistsLib.Types.IPlaylist _playlist = null;
        private DownloadState _downloadState = DownloadState.None;
        public event Action<IGenericEntry> FinishedDownload;

        public abstract string Title { get; protected set; }
        public abstract string Author { get; protected set; }
        public abstract string Description { get; protected set; }
        public abstract string PlaylistURL { get; protected set; }

        public BeatSaberPlaylistsLib.Types.IPlaylist RemotePlaylist
        {
            get => _playlist;
            private set
            {
                _playlist = value;
                DownloadState = value == null ? DownloadState.None : DownloadState.DownloadedPlaylist;
            }
        }

        public BeatSaberPlaylistsLib.Types.IPlaylist LocalPlaylist { get; set; }

        public DownloadState DownloadState
        {
            get => _downloadState;
            set
            {
                _downloadState = value;
                if (value == DownloadState.DownloadedPlaylist)
                {
                    FinishedDownload?.Invoke(this);
                }
            }
        }

        public bool DownloadBlocked { get; set; }

        public abstract string SpriteString { get; protected set; }

        public abstract SpriteType SpriteType { get; }

        public async void DownloadPlaylist(SiraClient siraClient)
        {
            DownloadState = DownloadState.Downloading;
            try
            {
                WebResponse webResponse = await siraClient.GetAsync(PlaylistURL, CancellationToken.None);
                if (webResponse.IsSuccessStatusCode)
                {
                    Stream playlistStream = new MemoryStream(webResponse.ContentToBytes());
                    RemotePlaylist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(playlistStream);
                }
                else
                {
                    Plugin.Log.Info("An error occurred while acquiring " + PlaylistURL + $"\nError code: {webResponse.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Info("An exception occurred while acquiring " + PlaylistURL + $"\nException: {e.Message}");
            }
        }
    }

    public enum DownloadState { None, Downloading, DownloadedPlaylist, Downloaded };
    public enum SpriteType { URL, Base64 };
}
