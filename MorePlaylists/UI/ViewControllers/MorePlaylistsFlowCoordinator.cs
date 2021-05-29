using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using System;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace MorePlaylists.UI
{
    public class MorePlaylistsFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator mainFlowCoordinator;
        private MorePlaylistsNavigationController navigationController;
        private MorePlaylistsListViewController morePlaylistsListViewController;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, MorePlaylistsNavigationController navigationController, MorePlaylistsListViewController morePlaylistsListViewController)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.navigationController = navigationController;
            this.morePlaylistsListViewController = morePlaylistsListViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            SetTitle("Download Playlists");
            showBackButton = true;

            SetViewControllersToNavigationController(navigationController, morePlaylistsListViewController);
            ProvideInitialViewControllers(navigationController);
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
