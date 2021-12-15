using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using UnityEngine;
using MorePlaylists.Entries;
using MorePlaylists.Sources;
using MorePlaylists.Utilities;
using SiraUtil.Web;

namespace MorePlaylists.UI
{
    internal class MorePlaylistsFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private IHttpService siraHttpService;
        private MainFlowCoordinator mainFlowCoordinator;
        private MainMenuViewController mainMenuViewController;
        private SoloFreePlayFlowCoordinator soloFreePlayFlowCoordinator;
        private PopupModalsController popupModalsController;
        private SourceModalController sourceModalController;
        private MorePlaylistsNavigationController morePlaylistsNavigationController;
        private MorePlaylistsListViewController morePlaylistsListViewController;
        private MorePlaylistsDownloaderViewController morePlaylistsDownloaderViewController;
        private MorePlaylistsDetailViewController morePlaylistsDetailViewController;
        private MorePlaylistsSongListViewController morePlaylistsSongListViewController;

        [Inject]
        public void Construct(IHttpService siraHttpService, MainFlowCoordinator mainFlowCoordinator, MainMenuViewController mainMenuViewController, SoloFreePlayFlowCoordinator soloFreePlayFlowCoordinator,
            PopupModalsController popupModalsController, SourceModalController sourceModalController, MorePlaylistsNavigationController morePlaylistsNavigationController,
            MorePlaylistsListViewController morePlaylistsListViewController, MorePlaylistsDownloaderViewController morePlaylistsDownloaderViewController,
            MorePlaylistsDetailViewController morePlaylistsDetailViewController, MorePlaylistsSongListViewController morePlaylistsSongListViewController)
        {
            this.siraHttpService = siraHttpService;
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.mainMenuViewController = mainMenuViewController;
            this.soloFreePlayFlowCoordinator = soloFreePlayFlowCoordinator;
            this.popupModalsController = popupModalsController;
            this.sourceModalController = sourceModalController;
            this.morePlaylistsNavigationController = morePlaylistsNavigationController;
            this.morePlaylistsListViewController = morePlaylistsListViewController;
            this.morePlaylistsDownloaderViewController = morePlaylistsDownloaderViewController;
            this.morePlaylistsDetailViewController = morePlaylistsDetailViewController;
            this.morePlaylistsSongListViewController = morePlaylistsSongListViewController;
        }

        public void Initialize()
        {
            sourceModalController.DidSelectSource += SourceSelected;

            morePlaylistsListViewController.DidSelectPlaylist += PlaylistSelected;
            morePlaylistsListViewController.DidClickSource += SourceButtonClicked;
            morePlaylistsListViewController.DidClickSearch += SearchButtonClicked;

            morePlaylistsDetailViewController.DidPressDownload += morePlaylistsDownloaderViewController.DownloadPlaylist;
            morePlaylistsDetailViewController.DidGoToPlaylist += GoToPlaylistClicked;

            morePlaylistsDownloaderViewController.PlaylistDownloaded += OnPlaylistDownloaded;
        }

        public void Dispose()
        {
            sourceModalController.DidSelectSource -= SourceSelected;

            morePlaylistsListViewController.DidSelectPlaylist -= PlaylistSelected;
            morePlaylistsListViewController.DidClickSource -= SourceButtonClicked;
            morePlaylistsListViewController.DidClickSearch -= SearchButtonClicked;

            morePlaylistsDetailViewController.DidPressDownload -= morePlaylistsDownloaderViewController.DownloadPlaylist;
            morePlaylistsDetailViewController.DidGoToPlaylist -= GoToPlaylistClicked;

            morePlaylistsDownloaderViewController.PlaylistDownloaded -= OnPlaylistDownloaded;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Download Playlists");
            showBackButton = true;

            SetViewControllersToNavigationController(morePlaylistsNavigationController, morePlaylistsListViewController);
            ProvideInitialViewControllers(morePlaylistsNavigationController, morePlaylistsDownloaderViewController, morePlaylistsSongListViewController);
        }

        private void SourceSelected(ISource source)
        {
            morePlaylistsListViewController.ShowPlaylistsForSource(source);
            morePlaylistsSongListViewController.ClearList();
            if (morePlaylistsDetailViewController.isInViewControllerHierarchy)
            {
                PopViewControllerFromNavigationController(morePlaylistsNavigationController, immediately: true);
            }
        }

        private void PlaylistSelected(GenericEntry selectedPlaylistEntry)
        {
            if (selectedPlaylistEntry.DownloadState == DownloadState.None)
            {
                selectedPlaylistEntry.DownloadPlaylist(siraHttpService);
            }

            if (!morePlaylistsDetailViewController.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(morePlaylistsNavigationController, morePlaylistsDetailViewController, DetailViewPushed, true);
            }
            morePlaylistsDetailViewController.ShowDetail(selectedPlaylistEntry);
            morePlaylistsSongListViewController.SetCurrentPlaylist(selectedPlaylistEntry);
        }

        private void SourceButtonClicked() => sourceModalController.ShowModal(morePlaylistsListViewController.transform);

        private void SearchButtonClicked() => popupModalsController.ShowKeyboard(morePlaylistsListViewController.transform, morePlaylistsListViewController.Search);

        private void OnPlaylistDownloaded(IGenericEntry playlistEntry)
        {
            if (playlistEntry.DownloadState == DownloadState.Downloaded)
            {
                morePlaylistsListViewController.SetEntryAsOwned(playlistEntry);
            }
            else
            {
                popupModalsController.ShowOkModal(morePlaylistsListViewController.transform, "An error occured while downloading, please try again later", null);
            }
            morePlaylistsDetailViewController.OnPlaylistDownloaded();
        }

        #region Go To Playlist

        private void GoToPlaylistClicked(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            morePlaylistsListViewController.AbortLoading();
            morePlaylistsSongListViewController.SetLoading(false);
            mainFlowCoordinator.DismissFlowCoordinator(this, immediately: true);
            soloFreePlayFlowCoordinator.Setup(Utils.GetStateForPlaylist(playlist));
            mainMenuViewController.HandleMenuButton(MainMenuViewController.MenuButton.SoloFreePlay);
        }

        #endregion

        private void DetailViewPushed() => morePlaylistsDetailViewController.transform.localPosition = new Vector3(45, 0, 0);

        #region Back Button Pressed

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            morePlaylistsListViewController.AbortLoading();
            morePlaylistsSongListViewController.SetLoading(false);
            mainFlowCoordinator.DismissFlowCoordinator(this);
        }

        #endregion
    }

    public class MorePlaylistsNavigationController : NavigationController
    {
    }
}
