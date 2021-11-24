﻿using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using UnityEngine;
using MorePlaylists.Entries;
using MorePlaylists.Sources;
using IPA.Utilities;

namespace MorePlaylists.UI
{
    internal class MorePlaylistsFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private MainFlowCoordinator mainFlowCoordinator;
        private MainMenuViewController mainMenuViewController;
        private LevelFilteringNavigationController levelFilteringNavigationController;
        private SelectLevelCategoryViewController selectLevelCategoryViewController;
        private IconSegmentedControl levelCategorySegmentedControl;
        private PopupModalsController popupModalsController;
        private SourceModalController sourceModalController;
        private MorePlaylistsNavigationController morePlaylistsNavigationController;
        private MorePlaylistsListViewController morePlaylistsListViewController;
        private MorePlaylistsDownloaderViewController morePlaylistsDownloadQueueViewController;
        private MorePlaylistsDetailViewController morePlaylistsDetailViewController;
        private MorePlaylistsSongListViewController morePlaylistsSongListViewController;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, MainMenuViewController mainMenuViewController, LevelFilteringNavigationController levelFilteringNavigationController, SelectLevelCategoryViewController selectLevelCategoryViewController,
            PopupModalsController popupModalsController, SourceModalController sourceModalController, MorePlaylistsNavigationController morePlaylistsNavigationController, MorePlaylistsListViewController morePlaylistsListViewController, 
            MorePlaylistsDownloaderViewController morePlaylistsDownloadQueueViewController, MorePlaylistsDetailViewController morePlaylistsDetailViewController, MorePlaylistsSongListViewController morePlaylistsSongListViewController)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.mainMenuViewController = mainMenuViewController;
            this.levelFilteringNavigationController = levelFilteringNavigationController;
            this.selectLevelCategoryViewController = selectLevelCategoryViewController;
            levelCategorySegmentedControl = FieldAccessor<SelectLevelCategoryViewController, IconSegmentedControl>.Get(ref selectLevelCategoryViewController, "_levelFilterCategoryIconSegmentedControl");
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
            morePlaylistsDetailViewController.DidGoToPlaylist += MorePlaylistsDetailViewController_DidGoToPlaylist;
        }

        public void Dispose()
        {
            sourceModalController.DidSelectSource -= SourceModalController_DidSelectSource;

            morePlaylistsListViewController.DidSelectPlaylist -= MorePlaylistsListViewController_DidSelectPlaylist;
            morePlaylistsListViewController.DidClickSource -= MorePlaylistsListViewController_DidClickSource;
            morePlaylistsListViewController.DidClickSearch -= MorePlaylistsListViewController_DidClickSearch;

            morePlaylistsDetailViewController.DidPressDownload -= MorePlaylistsDetailViewController_DidPressDownload;
            morePlaylistsDetailViewController.DidGoToPlaylist -= MorePlaylistsDetailViewController_DidGoToPlaylist;
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
            morePlaylistsSongListViewController.ClearList();
            if (morePlaylistsDetailViewController.isInViewControllerHierarchy)
            {
                PopViewControllerFromNavigationController(morePlaylistsNavigationController, immediately: true);
            }
        }

        private void MorePlaylistsListViewController_DidSelectPlaylist(GenericEntry selectedPlaylistEntry)
        {
            if (selectedPlaylistEntry.DownloadState == DownloadState.None || selectedPlaylistEntry.DownloadState == DownloadState.Error)
            {
                _ = selectedPlaylistEntry.RemotePlaylist;
            }

            if (!morePlaylistsDetailViewController.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(morePlaylistsNavigationController, morePlaylistsDetailViewController, DetailViewPushed, true);
            }
            morePlaylistsDetailViewController.ShowDetail(selectedPlaylistEntry);
            morePlaylistsSongListViewController.SetCurrentPlaylist(selectedPlaylistEntry);
        }

        private void MorePlaylistsListViewController_DidClickSource() => sourceModalController.ShowModal(morePlaylistsListViewController.transform);

        private void MorePlaylistsListViewController_DidClickSearch() => popupModalsController.ShowKeyboard(morePlaylistsListViewController.transform, morePlaylistsListViewController.Search);

        private void MorePlaylistsDetailViewController_DidPressDownload(IGenericEntry playlistEntry, bool downloadSongs)
        {
            playlistEntry.DownloadBlocked = true;
        }

        #region Go To Playlist

        private void MorePlaylistsDetailViewController_DidGoToPlaylist(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
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
