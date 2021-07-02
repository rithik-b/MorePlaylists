using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using System;
using UnityEngine;

namespace MorePlaylists.UI
{
    public class MorePlaylistsDetailViewController : BSMLResourceViewController
    {
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsDetailView.bsml";
        private IGenericEntry selectedPlaylistEntry;

        internal event Action<IGenericEntry, bool> DidPressDownload;
        internal event Action<BeatSaberPlaylistsLib.Types.IPlaylist> DidGoToPlaylist;

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView;

        [UIComponent("text-page")]
        private readonly TextPageScrollView descriptionTextPage;

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

        internal void ShowDetail(IGenericEntry selectedPlaylistEntry)
        {
            this.selectedPlaylistEntry = selectedPlaylistEntry;
            NotifyPropertyChanged(nameof(DownloadInteractable));
            NotifyPropertyChanged(nameof(DownloadActive));
            NotifyPropertyChanged(nameof(GoToActive));
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(PlaylistAuthor));
            NotifyPropertyChanged(nameof(PlaylistDescription));
            playlistCoverView.sprite = selectedPlaylistEntry.Sprite;
            descriptionTextPage.ScrollTo(0, true);
        }

        internal void OnPlaylistDownloaded()
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
        private string PlaylistDescription => selectedPlaylistEntry?.Description ?? "";

        [UIValue("download-interactable")]
        public bool DownloadInteractable => selectedPlaylistEntry is {DownloadBlocked: false};

        [UIValue("download-active")]
        public bool DownloadActive => selectedPlaylistEntry is {LocalPlaylist: null};

        [UIValue("go-to-active")]
        public bool GoToActive => selectedPlaylistEntry is {LocalPlaylist: { }};

        #endregion
    }
}
