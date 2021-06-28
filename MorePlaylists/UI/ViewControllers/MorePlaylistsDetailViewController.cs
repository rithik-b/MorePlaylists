using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using System;
using System.ComponentModel;
using UnityEngine;

namespace MorePlaylists.UI
{
    public class MorePlaylistsDetailViewController : BSMLResourceViewController, INotifyPropertyChanged
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
            (transform as RectTransform).sizeDelta = new Vector2(70, 0);
            (transform as RectTransform).anchorMin = new Vector2(0.5f, 0);
            (transform as RectTransform).anchorMax = new Vector2(0.5f, 1);
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

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        internal void ShowDetail(IGenericEntry selectedPlaylist)
        {
            this.selectedPlaylistEntry = selectedPlaylist;
            NotifyPropertyChanged(nameof(DownloadInteractable));
            NotifyPropertyChanged(nameof(DownloadActive));
            NotifyPropertyChanged(nameof(GoToActive));
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(PlaylistAuthor));
            NotifyPropertyChanged(nameof(PlaylistDescription));
            playlistCoverView.sprite = selectedPlaylist.Sprite;
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
        public string PlaylistName => selectedPlaylistEntry == null || selectedPlaylistEntry.Title == null ? " " : selectedPlaylistEntry.Title;

        [UIValue("playlist-author")]
        public string PlaylistAuthor => selectedPlaylistEntry == null || selectedPlaylistEntry.Author == null ? " " : selectedPlaylistEntry.Author;

        [UIValue("playlist-description")]
        private string PlaylistDescription => selectedPlaylistEntry == null || selectedPlaylistEntry.Description == null ? "" : selectedPlaylistEntry.Description;

        [UIValue("download-interactable")]
        public bool DownloadInteractable => selectedPlaylistEntry != null && !selectedPlaylistEntry.DownloadBlocked;

        [UIValue("download-active")]
        public bool DownloadActive => selectedPlaylistEntry != null && selectedPlaylistEntry.LocalPlaylist == null;

        [UIValue("go-to-active")]
        public bool GoToActive => selectedPlaylistEntry != null && selectedPlaylistEntry.LocalPlaylist != null;

        #endregion
    }
}
