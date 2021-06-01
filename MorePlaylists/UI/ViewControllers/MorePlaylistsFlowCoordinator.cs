using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using BeatSaberPlaylistsLib.Legacy;
using UnityEngine;
using MorePlaylists.Types;

namespace MorePlaylists.UI
{
    public class MorePlaylistsFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private MainFlowCoordinator mainFlowCoordinator;
        private MorePlaylistsNavigationController morePlaylistsNavigationController;
        private MorePlaylistsListViewController morePlaylistsListViewController;
        private MorePlaylistsDownloadQueueViewController morePlaylistsDownloadQueueViewController;
        private MorePlaylistsDetailViewController morePlaylistsDetailViewController;
        private MorePlaylistsSongListViewController morePlaylistsSongListViewController;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, MorePlaylistsNavigationController morePlaylistsNavigationController, MorePlaylistsListViewController morePlaylistsListViewController, 
            MorePlaylistsDownloadQueueViewController morePlaylistsDownloadQueueViewController, MorePlaylistsDetailViewController morePlaylistsDetailViewController, MorePlaylistsSongListViewController morePlaylistsSongListViewController)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.morePlaylistsNavigationController = morePlaylistsNavigationController;
            this.morePlaylistsListViewController = morePlaylistsListViewController;
            this.morePlaylistsDownloadQueueViewController = morePlaylistsDownloadQueueViewController;
            this.morePlaylistsDetailViewController = morePlaylistsDetailViewController;
            this.morePlaylistsSongListViewController = morePlaylistsSongListViewController;
        }

        public void Initialize()
        {
            morePlaylistsListViewController.didSelectPlaylist += MorePlaylistsListViewController_DidSelectPlaylist;
            morePlaylistsDetailViewController.didPressDownload += MorePlaylistsDetailViewController_DidPressDownload;
        }

        public void Dispose()
        {
            morePlaylistsListViewController.didSelectPlaylist -= MorePlaylistsListViewController_DidSelectPlaylist;
            morePlaylistsDetailViewController.didPressDownload -= MorePlaylistsDetailViewController_DidPressDownload;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Download Playlists");
            showBackButton = true;

            SetViewControllersToNavigationController(morePlaylistsNavigationController, morePlaylistsListViewController);
            ProvideInitialViewControllers(morePlaylistsNavigationController, morePlaylistsDownloadQueueViewController, morePlaylistsSongListViewController);
        }

        private void MorePlaylistsListViewController_DidSelectPlaylist(GenericEntry selectedPlaylist)
        {
            if (!morePlaylistsDetailViewController.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(morePlaylistsNavigationController, morePlaylistsDetailViewController, DetailViewPushed, true);
            }
            morePlaylistsDetailViewController.ShowDetail(selectedPlaylist);
        }

        private void MorePlaylistsDetailViewController_DidPressDownload(IGenericEntry playlistToDownload)
        {
            morePlaylistsDownloadQueueViewController.EnqueuePlaylist(playlistToDownload);
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
