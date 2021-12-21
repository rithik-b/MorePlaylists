using System;
using System.Collections.Generic;
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
        private IHttpService siraHttpService;
        private PlaylistDownloader playlistDownloader;
        private PlaylistDownloaderViewController playlistDownloaderViewController;
        private HashSet<IGenericEntry> DownloadSongs;
        public Action<IGenericEntry> PlaylistDownloaded;

        [Inject]
        public void Construct(IHttpService siraHttpService, PlaylistDownloader playlistDownloader, PlaylistDownloaderViewController playlistDownloaderViewController)
        {
            this.siraHttpService = siraHttpService;
            this.playlistDownloader = playlistDownloader;
            this.playlistDownloaderViewController = playlistDownloaderViewController;
            DownloadSongs = new HashSet<IGenericEntry>();
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            playlistDownloaderViewController.SetParent(transform);
        }

        public void DownloadPlaylist(IGenericEntry playlistEntry, bool downloadSongs)
        {
            playlistEntry.DownloadBlocked = true;

            if (downloadSongs)
            {
                DownloadSongs.Add(playlistEntry);
            }

            if (playlistEntry.DownloadState == DownloadState.Downloaded)
            {
                DownloadFinished(playlistEntry);
            }
            else
            {
                playlistEntry.FinishedDownload += DownloadFinished;
                if (playlistEntry.DownloadState == DownloadState.None)
                {
                    playlistEntry.DownloadPlaylist(siraHttpService);
                }
            }
        }

        private void DownloadFinished(IGenericEntry playlistEntry)
        {
            playlistEntry.FinishedDownload -= DownloadFinished;

            if (playlistEntry.DownloadState == DownloadState.Downloaded)
            {
                PlaylistLibUtils.SavePlaylist(playlistEntry);
                if (DownloadSongs.Contains(playlistEntry))
                {
                    playlistDownloader.QueuePlaylist(new PlaylistManager.Types.DownloadQueueEntry(playlistEntry.LocalPlaylist, BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetManagerForPlaylist(playlistEntry.LocalPlaylist)));
                }
            }

            if (DownloadSongs.Contains(playlistEntry))
            {
                DownloadSongs.Remove(playlistEntry);
            }
            PlaylistDownloaded?.Invoke(playlistEntry);
        }
    }
}
