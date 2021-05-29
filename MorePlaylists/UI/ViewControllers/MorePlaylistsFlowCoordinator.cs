using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using BeatSaberPlaylistsLib.Legacy;

namespace MorePlaylists.UI
{
    public class MorePlaylistsFlowCoordinator : FlowCoordinator, IInitializable, IDisposable
    {
        private MainFlowCoordinator mainFlowCoordinator;
        private MorePlaylistsNavigationController morePlaylistsNavigationController;
        private MorePlaylistsListViewController morePlaylistsListViewController;
        private MorePlaylistsDetailViewController morePlaylistsDetailViewController;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, MorePlaylistsNavigationController navigationController, MorePlaylistsListViewController morePlaylistsListViewController, MorePlaylistsDetailViewController morePlaylistsDetailViewController)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.morePlaylistsNavigationController = navigationController;
            this.morePlaylistsListViewController = morePlaylistsListViewController;
            this.morePlaylistsDetailViewController = morePlaylistsDetailViewController;
        }

        public void Initialize()
        {
            morePlaylistsListViewController.didSelectPlaylist += MorePlaylistsListViewController_DidSelectPlaylist;
        }

        public void Dispose()
        {
            morePlaylistsListViewController.didSelectPlaylist -= MorePlaylistsListViewController_DidSelectPlaylist;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Download Playlists");
            showBackButton = true;

            SetViewControllersToNavigationController(morePlaylistsNavigationController, morePlaylistsListViewController);
            ProvideInitialViewControllers(morePlaylistsNavigationController);
        }

        private void MorePlaylistsListViewController_DidSelectPlaylist(LegacyPlaylist selectedPlaylist)
        {
            if (!morePlaylistsDetailViewController.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(morePlaylistsNavigationController, morePlaylistsDetailViewController);
            }
            morePlaylistsDetailViewController.ShowDetail(selectedPlaylist);
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
