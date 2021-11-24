using HMUI;
using PlaylistManager.UI;
using Zenject;

namespace MorePlaylists.UI
{
    internal class MorePlaylistsDownloaderViewController : ViewController
    {
        private PlaylistDownloaderViewController playlistDownloaderViewController;

        [Inject]
        public void Construct(PlaylistDownloaderViewController playlistDownloaderViewController)
        {
            this.playlistDownloaderViewController = playlistDownloaderViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            playlistDownloaderViewController.SetParent(transform);
        }
    }
}
