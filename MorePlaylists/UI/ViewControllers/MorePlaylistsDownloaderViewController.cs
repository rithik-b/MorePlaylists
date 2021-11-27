using System;
using System.Collections.Generic;
using HMUI;
using MorePlaylists.Entries;
using PlaylistManager.UI;
using PlaylistManager.Utilities;
using Zenject;
using PlaylistLibUtils = MorePlaylists.Utilities.PlaylistLibUtils;

namespace MorePlaylists.UI
{
    internal class MorePlaylistsDownloaderViewController : ViewController
    {
        private PlaylistDownloader playlistDownloader;
        private PlaylistDownloaderViewController playlistDownloaderViewController;
        private HashSet<IGenericEntry> DownloadSongs;
        public Action<IGenericEntry> PlaylistDownloaded;

        [Inject]
        public void Construct(PlaylistDownloader playlistDownloader, PlaylistDownloaderViewController playlistDownloaderViewController)
        {
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

            if (playlistEntry.DownloadState == DownloadState.DownloadedPlaylist)
            {
                DownloadFinished(playlistEntry);
            }
            else
            {
                playlistEntry.FinishedDownload += DownloadFinished;
            }
        }

        private void DownloadFinished(IGenericEntry playlistEntry)
        {
            playlistEntry.FinishedDownload -= DownloadFinished;
            PlaylistLibUtils.SavePlaylist(playlistEntry);
            if (DownloadSongs.Contains(playlistEntry))
            {
                playlistDownloader.QueuePlaylist(new PlaylistManager.Types.DownloadQueueEntry(playlistEntry.LocalPlaylist, BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetManagerForPlaylist(playlistEntry.LocalPlaylist)));
                DownloadSongs.Remove(playlistEntry);
            }
            PlaylistDownloaded?.Invoke(playlistEntry);
        }
    }
}
