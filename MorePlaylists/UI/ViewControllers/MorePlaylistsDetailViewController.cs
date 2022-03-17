using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using System;
using BeatSaberPlaylistsLib.Types;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\MorePlaylistsDetailView.bsml")]
    [ViewDefinition("MorePlaylists.UI.Views.MorePlaylistsDetailView.bsml")]
    internal class MorePlaylistsDetailViewController : BSMLAutomaticViewController, IDetailViewController
    {
        private IEntry? selectedPlaylistEntry;
        private SpriteLoader spriteLoader;
        
        public ViewController ViewController => this;
        public event Action<IEntry, bool>? DidPressDownload;
        public event Action<IPlaylist>? DidGoToPlaylist;

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView;

        [UIComponent("text-page")]
        private readonly TextPageScrollView descriptionTextPage;

        [Inject]
        public void Construct(SpriteLoader spriteLoader)
        {
            this.spriteLoader = spriteLoader;
        }

        #region Actions

        [UIAction("#post-parse")]
        private void PostParse()
        {
            var rectTransform = (RectTransform) transform;
            rectTransform.sizeDelta = new Vector2(70, 0);
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
        }

        [UIAction("download-click")]
        private void DownloadPressed()
        {
            DidPressDownload?.Invoke(selectedPlaylistEntry, false);
            NotifyPropertyChanged(nameof(DownloadInteractable));
        }

        [UIAction("download-all-click")]
        private void DownloadAllPressed()
        {
            DidPressDownload?.Invoke(selectedPlaylistEntry, true);
            NotifyPropertyChanged(nameof(DownloadInteractable));
        }

        [UIAction("go-to-playlist")]
        private void GoToPlaylist()
        {
            if (selectedPlaylistEntry.LocalPlaylist != null)
            {
                DidGoToPlaylist?.Invoke(selectedPlaylistEntry.LocalPlaylist);
            }
        }

        #endregion

        public void ShowDetail(IEntry selectedPlaylistEntry)
        {
            this.selectedPlaylistEntry = selectedPlaylistEntry;
            NotifyPropertyChanged(nameof(DownloadInteractable));
            NotifyPropertyChanged(nameof(DownloadActive));
            NotifyPropertyChanged(nameof(GoToActive));
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(PlaylistAuthor));
            NotifyPropertyChanged(nameof(PlaylistDescription));
            spriteLoader.GetSpriteForEntry(selectedPlaylistEntry, (Sprite sprite) => playlistCoverView.sprite = sprite);
            descriptionTextPage.ScrollTo(0, true);
        }

        public void OnPlaylistDownloaded()
        {
            NotifyPropertyChanged(nameof(DownloadInteractable));
            NotifyPropertyChanged(nameof(DownloadActive));
            NotifyPropertyChanged(nameof(GoToActive));
        }

        #region Values

        [UIValue("playlist-name")]
        public string PlaylistName => selectedPlaylistEntry?.Title ?? " ";

        [UIValue("playlist-author")]
        public string PlaylistAuthor => selectedPlaylistEntry?.Author ?? " ";

        [UIValue("playlist-description")]
        private string PlaylistDescription => string.IsNullOrWhiteSpace(selectedPlaylistEntry?.Description) ? "No Description available for this playlist." : selectedPlaylistEntry?.Description;

        [UIValue("download-interactable")]
        public bool DownloadInteractable => selectedPlaylistEntry is {DownloadBlocked: false};

        [UIValue("download-active")]
        public bool DownloadActive => selectedPlaylistEntry is {LocalPlaylist: null};

        [UIValue("go-to-active")]
        public bool GoToActive => selectedPlaylistEntry is {LocalPlaylist: { }};

        #endregion
    }
}
