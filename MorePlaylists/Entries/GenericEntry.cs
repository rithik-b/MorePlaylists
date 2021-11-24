using MorePlaylists.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MorePlaylists.Entries
{
    public abstract class GenericEntry : IGenericEntry
    {
        private BeatSaberPlaylistsLib.Types.IPlaylist _playlist = null;
        private DownloadState _downloadState = DownloadState.None;
        public event Action FinishedDownload;

        public abstract string Title { get; protected set; }
        public abstract string Author { get; protected set; }
        public abstract string Description { get; protected set; }
        public abstract string PlaylistURL { get; protected set; }

        public BeatSaberPlaylistsLib.Types.IPlaylist RemotePlaylist
        {
            get
            {
                if (_playlist == null && DownloadState != DownloadState.Downloading)
                {
                    DownloadState = DownloadState.Downloading;
                    DownloadPlaylist();
                }

                return _playlist;
            }
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
                if (value == DownloadState.DownloadedPlaylist || value == DownloadState.Error)
                {
                    FinishedDownload?.Invoke();
                }
            }
        }

        public bool DownloadBlocked { get; set; }

        public abstract string SpriteString { get; protected set; }

        public abstract SpriteType SpriteType { get; }

        private async void DownloadPlaylist()
        {
            try
            {
                Stream playlistStream = new MemoryStream(await DownloaderUtils.instance.DownloadFileToBytesAsync(PlaylistURL));
                RemotePlaylist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(playlistStream);
            }
            catch (Exception e)
            {
                if (!(e is TaskCanceledException || e.Message.ToUpper().Contains("ABORTED")))
                {
                    Plugin.Log.Critical("An exception occurred while acquiring " + PlaylistURL + "\nException: " + e.Message);
                    DownloadState = DownloadState.Error;
                }
                else
                {
                    DownloadState = DownloadState.None;
                }
            }
        }
    }

    public enum DownloadState { None, Downloading, DownloadedPlaylist, Downloaded, Error };
    public enum SpriteType { URL, Base64 };
}
