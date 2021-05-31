using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Legacy;
using HMUI;
using MorePlaylists.Types;
using MorePlaylists.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePlaylists.UI
{
    public class MorePlaylistsDetailViewController : BSMLResourceViewController, INotifyPropertyChanged
    {
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsDetailView.bsml";
        private IGenericEntry selectedPlaylist;

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView;

        [UIComponent("text-page")]
        private readonly TextPageScrollView descriptionTextPage;

        [UIValue("playlist-name")]
        public string PlaylistName => selectedPlaylist == null || selectedPlaylist.Title == null ? " " : selectedPlaylist.Title;

        [UIValue("playlist-author")]
        public string PlaylistAuthor => selectedPlaylist == null || selectedPlaylist.Author == null ? " " : selectedPlaylist.Author;

        [UIValue("playlist-description")]
        private string PlaylistDescription => selectedPlaylist == null || selectedPlaylist.Description == null ? "" : selectedPlaylist.Description;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            (transform as RectTransform).sizeDelta = new Vector2(70, 0);
            (transform as RectTransform).anchorMin = new Vector2(0.5f, 0);
            (transform as RectTransform).anchorMax = new Vector2(0.5f, 1);
        }

        [UIAction("download-click")]
        private async Task DownloadPlaylistAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            try
            {
                Stream playlistStream = new MemoryStream(await DownloaderUtils.instance.DownloadFileToBytesAsync(selectedPlaylist.PlaylistURL, tokenSource.Token));
                BeatSaberPlaylistsLib.Types.IPlaylist playlist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(playlistStream);
                PlaylistLibUtils.SavePlaylist(playlist);
            }
            catch (Exception e)
            {
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        internal void ShowDetail(IGenericEntry selectedPlaylist)
        {
            this.selectedPlaylist = selectedPlaylist;
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(PlaylistAuthor));
            NotifyPropertyChanged(nameof(PlaylistDescription));
            playlistCoverView.sprite = selectedPlaylist.Sprite;
            descriptionTextPage.ScrollTo(0, true);
        }
    }
}
