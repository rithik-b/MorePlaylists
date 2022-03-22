using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using UnityEngine;
using MorePlaylists.Entries;
using MorePlaylists.Sources;
using MorePlaylists.Utilities;

namespace MorePlaylists.UI
{
    internal class MorePlaylistsFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        [Inject]
        private readonly MainFlowCoordinator mainFlowCoordinator = null!;

        [Inject]
        private readonly MainMenuViewController mainMenuViewController = null!;

        [Inject] 
        private readonly SoloFreePlayFlowCoordinator soloFreePlayFlowCoordinator = null!;
        
        [Inject]
        private readonly PopupModalsController popupModalsController = null!;
        
        [Inject]
        private readonly SourceModalController sourceModalController = null!;
        
        [Inject]
        private readonly MorePlaylistsNavigationController morePlaylistsNavigationController = null!;

        [Inject]
        private readonly MorePlaylistsDownloaderViewController morePlaylistsDownloaderViewController = null!;

        [Inject]
        private readonly MorePlaylistsSongListViewController morePlaylistsSongListViewController = null!;

        private IListViewController ListViewController => sourceModalController.SelectedSource.ListViewController;
        private IDetailViewController DetailViewController => sourceModalController.SelectedSource.DetailViewController;

        public void Initialize()
        {
            sourceModalController.DidSelectSource += SourceSelected;
            morePlaylistsDownloaderViewController.PlaylistDownloaded += OnPlaylistDownloaded;
        }

        public void Dispose()
        {
            sourceModalController.DidSelectSource -= SourceSelected;
            UnsubFromEvents(sourceModalController.SelectedSource);
            morePlaylistsDownloaderViewController.PlaylistDownloaded -= OnPlaylistDownloaded;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Download Playlists");
            showBackButton = true;
            SourceSelected(sourceModalController.SelectedSource);
            ProvideInitialViewControllers(morePlaylistsNavigationController, morePlaylistsDownloaderViewController);
        }

        private void SourceSelected(ISource source)
        {
            UnsubFromEvents(sourceModalController.SelectedSource);
            SubToEvents(source);
            source.ListViewController.ShowPlaylistsForSource(source);
            SetViewControllersToNavigationController(morePlaylistsNavigationController, source.ListViewController.ViewController);
        }

        private void ShowViewController(ViewController viewController, ViewController.AnimationDirection animationDirection)
        {
            showBackButton = false;
            PresentViewController(viewController, animationDirection: animationDirection);
        }

        private void DismissViewController(ViewController viewController, ViewController.AnimationDirection animationDirection, Action? finishedCallback)
        {
            if (viewController.isInViewControllerHierarchy)
            {
                showBackButton = true;
                base.DismissViewController(viewController, animationDirection, finishedCallback);
            }
        }

        private void PlaylistSelected(IEntry selectedPlaylistEntry)
        {
            if (!DetailViewController.ViewController.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(morePlaylistsNavigationController, DetailViewController.ViewController);
            }
            DetailViewController.ShowDetail(selectedPlaylistEntry);
            SetRightScreenViewController(morePlaylistsSongListViewController, ViewController.AnimationType.In);
            morePlaylistsSongListViewController.SetCurrentPlaylist(selectedPlaylistEntry);
        }

        private void SourceButtonClicked() => sourceModalController.ShowModal(ListViewController.ViewController.transform);
        
        private void DismissDetailView()
        {
            if (DetailViewController.ViewController.isInViewControllerHierarchy)
            {
                PopViewControllerFromNavigationController(morePlaylistsNavigationController, immediately: true);
            }

            if (morePlaylistsSongListViewController.isInViewControllerHierarchy)
            {
                SetRightScreenViewController(null, ViewController.AnimationType.Out);
            }
        }
        
        private void OnPlaylistDownloaded(IEntry playlistEntry)
        {
            if (playlistEntry.DownloadBlocked)
            {
                ListViewController.SetEntryAsOwned(playlistEntry);
            }
            else
            {
                popupModalsController.ShowOkModal(ListViewController.ViewController.transform, "An error occured while downloading, please try again later", null);
            }
            DetailViewController.OnPlaylistDownloaded();
        }

        private void GoToPlaylistClicked(BeatSaberPlaylistsLib.Types.IPlaylist playlist)
        {
            ListViewController.AbortLoading();
            mainFlowCoordinator.DismissFlowCoordinator(this, immediately: true);
            soloFreePlayFlowCoordinator.Setup(Utils.GetStateForPlaylist(playlist));
            mainMenuViewController.HandleMenuButton(MainMenuViewController.MenuButton.SoloFreePlay);
        }
        
        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            ListViewController.AbortLoading();
            mainFlowCoordinator.DismissFlowCoordinator(this);
        }

        private void SubToEvents(ISource source)
        {
            source.ViewControllerRequested += ShowViewController;
            source.ViewControllerDismissRequested += DismissViewController;
            
            source.ListViewController.DidSelectPlaylist += PlaylistSelected;
            source.ListViewController.DidClickSource += SourceButtonClicked;
            source.ListViewController.DetailDismissRequested += DismissDetailView;
            
            source.DetailViewController.DidPressDownload += morePlaylistsDownloaderViewController.DownloadPlaylist;
            source.DetailViewController.DidGoToPlaylist += GoToPlaylistClicked;
        }

        private void UnsubFromEvents(ISource source)
        {
            source.ViewControllerRequested -= ShowViewController;
            source.ViewControllerDismissRequested -= DismissViewController;
            
            source.ListViewController.DidSelectPlaylist -= PlaylistSelected;
            source.ListViewController.DidClickSource -= SourceButtonClicked;
            source.ListViewController.DetailDismissRequested -= DismissDetailView;
            
            source.DetailViewController.DidPressDownload -= morePlaylistsDownloaderViewController.DownloadPlaylist;
            source.DetailViewController.DidGoToPlaylist -= GoToPlaylistClicked;
        }
    }

    internal class MorePlaylistsNavigationController : NavigationController
    {
    }
}
