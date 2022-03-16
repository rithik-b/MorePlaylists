using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
                DownloadState = value == null ? DownloadState.None : DownloadState.Downloaded;
            }
        }

        public BeatSaberPlaylistsLib.Types.IPlaylist LocalPlaylist { get; set; }

        public DownloadState DownloadState
        {
            get => _downloadState;
            set
            {
                _downloadState = value;
                if (value == DownloadState.Downloaded || value == DownloadState.Error)
                {
                    FinishedDownload?.Invoke(this);
                }
            }
        }

        public bool DownloadBlocked { get; set; }

        public abstract string SpriteString { get; protected set; }

        public abstract SpriteType SpriteType { get; }

        public abstract Task<List<Song>> GetSongs(IHttpService siraHttpService);

        public async Task DownloadPlaylist(IHttpService siraHttpService)
        {
            DownloadState = DownloadState.Downloading;
            try
            {
                var webResponse = await siraHttpService.GetAsync(PlaylistURL, cancellationToken: CancellationToken.None);
                if (webResponse.Successful)
                {
                    RemotePlaylist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(await webResponse.ReadAsStreamAsync());
                }
                else
                {
                    Plugin.Log.Info("An error occurred while acquiring " + PlaylistURL + $"\nError code: {webResponse.Code}");
                    DownloadState = DownloadState.Error;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Info("An exception occurred while acquiring " + PlaylistURL + $"\nException: {e.Message}");
                DownloadState = DownloadState.Error;
            }
        }
    }

    public enum DownloadState { None, Downloading, Downloaded, Error };
    public enum SpriteType { URL, Base64, Playlist };
}
