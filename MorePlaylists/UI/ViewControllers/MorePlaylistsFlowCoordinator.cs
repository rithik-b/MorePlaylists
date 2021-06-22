using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using UnityEngine;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Sources;

namespace MorePlaylists.UI
{
    public class MorePlaylistsFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private MainFlowCoordinator mainFlowCoordinator;
        private PopupModalsController popupModalsController;
        private SourceModalController sourceModalController;
        private MorePlaylistsNavigationController morePlaylistsNavigationController;
        private MorePlaylistsListViewController morePlaylistsListViewController;
        private MorePlaylistsDownloadQueueViewController morePlaylistsDownloadQueueViewController;
        private MorePlaylistsDetailViewController morePlaylistsDetailViewController;
        private MorePlaylistsSongListViewController morePlaylistsSongListViewController;

        private CancellationTokenSource playlistDownloadTokenSource;
        private bool playlistDownloading = false;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, PopupModalsController popupModalsController, SourceModalController sourceModalController, MorePlaylistsNavigationController morePlaylistsNavigationController, MorePlaylistsListViewController morePlaylistsListViewController, 
            MorePlaylistsDownloadQueueViewController morePlaylistsDownloadQueueViewController, MorePlaylistsDetailViewController morePlaylistsDetailViewController, MorePlaylistsSongListViewController morePlaylistsSongListViewController)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.popupModalsController = popupModalsController;
            this.sourceModalController = sourceModalController;
            this.morePlaylistsNavigationController = morePlaylistsNavigationController;
            this.morePlaylistsListViewController = morePlaylistsListViewController;
            this.morePlaylistsDownloadQueueViewController = morePlaylistsDownloadQueueViewController;
            this.morePlaylistsDetailViewController = morePlaylistsDetailViewController;
            this.morePlaylistsSongListViewController = morePlaylistsSongListViewController;
        }

        public void Initialize()
        {
            sourceModalController.DidSelectSource += SourceModalController_DidSelectSource;
            morePlaylistsListViewController.DidSelectPlaylist += MorePlaylistsListViewController_DidSelectPlaylist;
            morePlaylistsListViewController.DidClickSource += MorePlaylistsListViewController_DidClickSource;
            morePlaylistsListViewController.DidClickSearch += MorePlaylistsListViewController_DidClickSearch;
            morePlaylistsDetailViewController.DidPressDownload += MorePlaylistsDetailViewController_DidPressDownload;
            MorePlaylistsDownloadQueueViewController.DidFinishDownloadingItem += MorePlaylistsDownloadQueueViewController_DidFinishDownloadingItem;
        }

        public void Dispose()
        {
            sourceModalController.DidSelectSource -= SourceModalController_DidSelectSource;
            morePlaylistsListViewController.DidSelectPlaylist -= MorePlaylistsListViewController_DidSelectPlaylist;
            morePlaylistsListViewController.DidClickSource -= MorePlaylistsListViewController_DidClickSource;
            morePlaylistsListViewController.DidClickSearch -= MorePlaylistsListViewController_DidClickSearch;
            morePlaylistsDetailViewController.DidPressDownload -= MorePlaylistsDetailViewController_DidPressDownload;
            MorePlaylistsDownloadQueueViewController.DidFinishDownloadingItem -= MorePlaylistsDownloadQueueViewController_DidFinishDownloadingItem;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Download Playlists");
            showBackButton = true;

            SetViewControllersToNavigationController(morePlaylistsNavigationController, morePlaylistsListViewController);
            ProvideInitialViewControllers(morePlaylistsNavigationController, morePlaylistsDownloadQueueViewController, morePlaylistsSongListViewController);
        }

        private void SourceModalController_DidSelectSource(ISource source)
        {
            morePlaylistsListViewController.ShowPlaylistsForSource(source);
            if (morePlaylistsDetailViewController.isInViewControllerHierarchy)
            {
                PopViewControllerFromNavigationController(morePlaylistsNavigationController, immediately: true);
            }
        }

        private void MorePlaylistsListViewController_DidSelectPlaylist(GenericEntry selectedPlaylistEntry)
        {
            if (!playlistDownloading)
            {
                playlistDownloadTokenSource?.Cancel();
                playlistDownloadTokenSource?.Dispose();
            }
            playlistDownloading = false;

            if (selectedPlaylistEntry.DownloadState == DownloadState.None || selectedPlaylistEntry.DownloadState == DownloadState.Error)
            {
                playlistDownloadTokenSource = new CancellationTokenSource();
                DownloadSelectedPlaylist(selectedPlaylistEntry);
            }
            else
            {
                playlistDownloadTokenSource = null;
            }

            if (!morePlaylistsDetailViewController.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(morePlaylistsNavigationController, morePlaylistsDetailViewController, DetailViewPushed, true);
            }
            morePlaylistsDetailViewController.ShowDetail(selectedPlaylistEntry);
            morePlaylistsSongListViewController.SetCurrentPlaylist(selectedPlaylistEntry);
        }

        private void MorePlaylistsListViewController_DidClickSource()
        {
            sourceModalController.ShowModal(morePlaylistsListViewController.transform);
        }

        private void MorePlaylistsListViewController_DidClickSearch() => popupModalsController.ShowKeyboard(morePlaylistsListViewController.transform, morePlaylistsListViewController.Search);

        private async void DownloadSelectedPlaylist(IGenericEntry playlistEntry)
        {
            playlistEntry.DownloadState = DownloadState.Downloading;
            try
            {
                Stream playlistStream = new MemoryStream(await DownloaderUtils.instance.DownloadFileToBytesAsync(playlistEntry.PlaylistURL, playlistDownloadTokenSource.Token));
                playlistEntry.Playlist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(playlistStream);
            }
            catch (Exception e)
            {
                if (!(e is TaskCanceledException || e.Message.ToUpper().Contains("ABORTED")))
                {
                    Plugin.Log.Critical("An exception occurred while acquiring " + playlistEntry.PlaylistURL + "\nException: " + e.Message);
                    playlistEntry.DownloadState = DownloadState.Error;
                }
                else
                {
                    playlistEntry.DownloadState = DownloadState.None;
                }
            }
        }

        private void MorePlaylistsDetailViewController_DidPressDownload(IGenericEntry playlistEntry, bool downloadSongs)
        {
            playlistDownloading = true;
            playlistEntry.Owned = true;
            morePlaylistsDownloadQueueViewController.EnqueuePlaylist(playlistEntry, playlistDownloadTokenSource, downloadSongs);
        }

        private void MorePlaylistsDownloadQueueViewController_DidFinishDownloadingItem(DownloadQueueItem item)
        {
            if (item.playlistEntry.DownloadState == DownloadState.Error)
            {
                popupModalsController.ShowOkModal(morePlaylistsListViewController.transform, "An error occured while downloading, please try again later", null);
            }
        }

        private void DetailViewPushed()
        {
            morePlaylistsDetailViewController.transform.localPosition = new Vector3(45, 0, 0);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            if (morePlaylistsDownloadQueueViewController.queueItems.Count != 0)
            {
                popupModalsController.ShowYesNoModal(morePlaylistsListViewController.transform, "There are still items in the download queue. Are you sure you want to cancel and exit?", ExitAndClear);
                return;
            }
            mainFlowCoordinator.DismissFlowCoordinator(this);
        }

        private void ExitAndClear()
        {
            morePlaylistsDownloadQueueViewController.queueItems.ForEach(x => (x as DownloadQueueItem).tokenSource.Cancel());
            morePlaylistsDownloadQueueViewController.queueItems.Clear();
            SongCore.Loader.Instance.RefreshSongs(false);
            mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }

    public class MorePlaylistsNavigationController : NavigationController
    {
    }
}
