using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using System;
using Zenject;

namespace MorePlaylists.UI
{
    public class MenuButtonUI : IInitializable, IDisposable
    {
        private readonly MenuButton menuButton;
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly MorePlaylistsFlowCoordinator morePlaylistsFlowCoordinator;

        public MenuButtonUI(MainFlowCoordinator mainFlowCoordinator, MorePlaylistsFlowCoordinator morePlaylistsFlowCoordinator)
        {
            menuButton = new MenuButton("More Playlists", "Download Playlists", MenuButtonClicked, true);
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.morePlaylistsFlowCoordinator = morePlaylistsFlowCoordinator;
        }

        public void Initialize()
        {
            MenuButtons.instance.RegisterButton(menuButton);
        }

        public void Dispose()
        {
            if (MenuButtons.IsSingletonAvailable)
            {
                MenuButtons.instance.UnregisterButton(menuButton);
            }
        }

        private void MenuButtonClicked()
        {
            mainFlowCoordinator.PresentFlowCoordinator(morePlaylistsFlowCoordinator, immediately: true);
        }
    }
}
