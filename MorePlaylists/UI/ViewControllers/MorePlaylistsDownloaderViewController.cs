using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using MorePlaylists.Entries;
using PlaylistManager.UI;
using PlaylistManager.Utilities;
using SiraUtil.Web;
using Zenject;
using PlaylistLibUtils = MorePlaylists.Utilities.PlaylistLibUtils;

namespace MorePlaylists.UI
{
    internal class MorePlaylistsDownloaderViewController : ViewController
    {
        [Inject] 
        private readonly IHttpService siraHttpService = null!;
        
        [Inject]
        private readonly PlaylistDownloader playlistDownloader = null!;
        
        [Inject]
        private readonly PlaylistDownloaderViewController playlistDownloaderViewController = null!;

        private readonly HashSet<IEntry> downloadSongs = new();
        public event Action<IEntry>? PlaylistDownloaded;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            playlistDownloaderViewController.SetParent(transform);
        }

        public void DownloadPlaylist(IEntry entry, bool downloadSongs)
        {
            entry.DownloadBlocked = true;

            if (downloadSongs)
            {
                this.downloadSongs.Add(entry);
            }

            _ = StartDownload(entry);
        }

        private async Task StartDownload(IEntry entry) => DownloadFinished(entry, await entry.DownloadPlaylist(siraHttpService));

        private void DownloadFinished(IEntry playlistEntry, IPlaylist? playlist)
        {
            if (playlist != null)
            {
                PlaylistLibUtils.SavePlaylist(playlistEntry, playlist);
                if (downloadSongs.Contains(playlistEntry))
                {
                    playlistDownloader.QueuePlaylist(new PlaylistManager.Types.DownloadQueueEntry(playlist, BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetManagerForPlaylist(playlist)));
                    downloadSongs.Remove(playlistEntry);
                }
            }
            else
            {
                playlistEntry.DownloadBlocked = false;
                if (downloadSongs.Contains(playlistEntry))
                {
                    downloadSongs.Remove(playlistEntry);
                }   
            }
            PlaylistDownloaded?.Invoke(playlistEntry);
        }
    }
}
