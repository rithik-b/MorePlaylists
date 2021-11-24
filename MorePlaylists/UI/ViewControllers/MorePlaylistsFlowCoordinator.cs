using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using UnityEngine;
using MorePlaylists.Entries;
using MorePlaylists.Sources;
using IPA.Utilities;
using PlaylistManager.Utilities;
using PlaylistLibUtils = MorePlaylists.Utilities.PlaylistLibUtils;
using SiraUtil;

namespace MorePlaylists.UI
{
    internal class MorePlaylistsFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private SiraClient siraClient;
        private MainFlowCoordinator mainFlowCoordinator;
        private MainMenuViewController mainMenuViewController;
        private LevelFilteringNavigationController levelFilteringNavigationController;
        private SelectLevelCategoryViewController selectLevelCategoryViewController;
        private IconSegmentedControl levelCategorySegmentedControl;
        private PopupModalsController popupModalsController;
        private SourceModalController sourceModalController;
        private MorePlaylistsNavigationController morePlaylistsNavigationController;
        private MorePlaylistsListViewController morePlaylistsListViewController;
        private MorePlaylistsDownloaderViewController morePlaylistsDownloaderViewController;
        private PlaylistDownloader playlistDownloader;
        private MorePlaylistsDetailViewController morePlaylistsDetailViewController;
        private MorePlaylistsSongListViewController morePlaylistsSongListViewController;

        [Inject]
        public void Construct(SiraClient siraClient, MainFlowCoordinator mainFlowCoordinator, MainMenuViewController mainMenuViewController, LevelFilteringNavigationController levelFilteringNavigationController,
            SelectLevelCategoryViewController selectLevelCategoryViewController, PopupModalsController popupModalsController, SourceModalController sourceModalController, MorePlaylistsNavigationController morePlaylistsNavigationController,
            MorePlaylistsListViewController morePlaylistsListViewController, MorePlaylistsDownloaderViewController morePlaylistsDownloaderViewController, PlaylistDownloader playlistDownloader,
            MorePlaylistsDetailViewController morePlaylistsDetailViewController, MorePlaylistsSongListViewController morePlaylistsSongListViewController)
        {
            this.siraClient = siraClient;
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.mainMenuViewController = mainMenuViewController;
            this.levelFilteringNavigationController = levelFilteringNavigationController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            levelCategorySegmentedControl = FieldAccessor<SelectLevelCategoryViewController, IconSegmentedControl>.Get(ref selectLevelCategoryViewController, "_levelFilterCategoryIconSegmentedControl");
            this.popupModalsController = popupModalsController;
            this.sourceModalController = sourceModalController;
            this.morePlaylistsNavigationController = morePlaylistsNavigationController;
            this.morePlaylistsListViewController = morePlaylistsListViewController;
            this.morePlaylistsDownloaderViewController = morePlaylistsDownloaderViewController;
            this.playlistDownloader = playlistDownloader;
            this.morePlaylistsDetailViewController = morePlaylistsDetailViewController;
            this.morePlaylistsSongListViewController = morePlaylistsSongListViewController;
        }

        public void Initialize()
        {
            sourceModalController.DidSelectSource += SourceSelected;

            morePlaylistsListViewController.DidSelectPlaylist += PlaylistSelected;
            morePlaylistsListViewController.DidClickSource += SourceButtonClicked;
            morePlaylistsListViewController.DidClickSearch += SearchButtonClicked;

            morePlaylistsDetailViewController.DidPressDownload += DownloadButtonClicked;
            morePlaylistsDetailViewController.DidGoToPlaylist += GoToPlaylistClicked;
        }

        public void Dispose()
        {
            sourceModalController.DidSelectSource -= SourceSelected;

            morePlaylistsListViewController.DidSelectPlaylist -= PlaylistSelected;
            morePlaylistsListViewController.DidClickSource -= SourceButtonClicked;
            morePlaylistsListViewController.DidClickSearch -= SearchButtonClicked;

            morePlaylistsDetailViewController.DidPressDownload -= DownloadButtonClicked;
            morePlaylistsDetailViewController.DidGoToPlaylist -= GoToPlaylistClicked;
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
                selectedPlaylistEntry.DownloadPlaylist(siraClient);
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

        private void DownloadButtonClicked(IGenericEntry playlistEntry, bool downloadSongs)
        {
            playlistEntry.DownloadBlocked = true;
            if (playlistEntry.DownloadState == DownloadState.DownloadedPlaylist)
            {
                DownloadFinished(playlistEntry);
                if (downloadSongs)
                {
                    playlistDownloader.QueuePlaylist(new PlaylistManager.Types.DownloadQueueEntry(playlistEntry.LocalPlaylist, BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.GetManagerForPlaylist(playlistEntry.LocalPlaylist)));
                }
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
            morePlaylistsDetailViewController.OnPlaylistDownloaded();
        }

        #region Go To Playlist

        private void GoToPlaylistClicked(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            morePlaylistsListViewController.AbortLoading();
            morePlaylistsSongListViewController.SetLoading(false);
            mainFlowCoordinator.DismissFlowCoordinator(this, immediately: true);
            mainMenuViewController.HandleMenuButton(MainMenuViewController.MenuButton.SoloFreePlay);
            levelCategorySegmentedControl.SelectCellWithNumber(1);
            selectLevelCategoryViewController.LevelFilterCategoryIconSegmentedControlDidSelectCell(levelCategorySegmentedControl, 1);
            levelFilteringNavigationController.SelectAnnotatedBeatmapLevelCollection(playlist);
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
