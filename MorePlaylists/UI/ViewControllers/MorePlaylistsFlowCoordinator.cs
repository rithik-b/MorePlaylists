using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using UnityEngine;
using MorePlaylists.Types;
using MorePlaylists.Utilities;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MorePlaylists.UI
{
    public class MorePlaylistsFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private MainFlowCoordinator mainFlowCoordinator;
        private PopupModalsController popupModalsController;
        private MorePlaylistsNavigationController morePlaylistsNavigationController;
        private MorePlaylistsListViewController morePlaylistsListViewController;
        private MorePlaylistsDownloadQueueViewController morePlaylistsDownloadQueueViewController;
        private MorePlaylistsDetailViewController morePlaylistsDetailViewController;
        private MorePlaylistsSongListViewController morePlaylistsSongListViewController;

        private CancellationTokenSource playlistDownloadTokenSource;
        private bool playlistDownloading = false;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, PopupModalsController popupModalsController, MorePlaylistsNavigationController morePlaylistsNavigationController, MorePlaylistsListViewController morePlaylistsListViewController, 
            MorePlaylistsDownloadQueueViewController morePlaylistsDownloadQueueViewController, MorePlaylistsDetailViewController morePlaylistsDetailViewController, MorePlaylistsSongListViewController morePlaylistsSongListViewController)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.popupModalsController = popupModalsController;
            this.morePlaylistsNavigationController = morePlaylistsNavigationController;
            this.morePlaylistsListViewController = morePlaylistsListViewController;
            this.morePlaylistsDownloadQueueViewController = morePlaylistsDownloadQueueViewController;
            this.morePlaylistsDetailViewController = morePlaylistsDetailViewController;
            this.morePlaylistsSongListViewController = morePlaylistsSongListViewController;
        }

        public void Initialize()
        {
            popupModalsController.DidSelectSource += PopupModalsController_DidSelectSource;
            morePlaylistsListViewController.DidSelectPlaylist += MorePlaylistsListViewController_DidSelectPlaylist;
            morePlaylistsListViewController.DidClickSource += MorePlaylistsListViewController_DidClickSource;
            morePlaylistsDetailViewController.DidPressDownload += MorePlaylistsDetailViewController_DidPressDownload;
        }

        public void Dispose()
        {
            popupModalsController.DidSelectSource -= PopupModalsController_DidSelectSource;
            morePlaylistsListViewController.DidSelectPlaylist -= MorePlaylistsListViewController_DidSelectPlaylist;
            morePlaylistsDetailViewController.DidPressDownload -= MorePlaylistsDetailViewController_DidPressDownload;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Download Playlists");
            showBackButton = true;

            SetViewControllersToNavigationController(morePlaylistsNavigationController, morePlaylistsListViewController);
            ProvideInitialViewControllers(morePlaylistsNavigationController, morePlaylistsDownloadQueueViewController, morePlaylistsSongListViewController);
        }

        private void PopupModalsController_DidSelectSource(DownloadSource downloadSource)
        {
            morePlaylistsListViewController.ShowPlaylistsForSource(downloadSource);
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

            if (selectedPlaylistEntry.Playlist == null)
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
            popupModalsController.ShowModal(morePlaylistsListViewController.transform);
        }

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
                if (!(e is TaskCanceledException))
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

        private void MorePlaylistsDetailViewController_DidPressDownload(IGenericEntry playlistEntry)
        {
            playlistDownloading = true;
            playlistEntry.Owned = true;
            morePlaylistsDownloadQueueViewController.EnqueuePlaylist(playlistEntry, playlistDownloadTokenSource);
        }

        private void DetailViewPushed()
        {
            morePlaylistsDetailViewController.transform.localPosition = new Vector3(45, 0, 0);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }

    public class MorePlaylistsNavigationController : NavigationController
    {
    }
}
