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
        private IGenericEntry selectedPlaylist;
        private bool _downloadInteractable = false;

        internal event Action<IGenericEntry, bool> DidPressDownload;

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
        private void DownloadPressed()
        {
            DidPressDownload?.Invoke(selectedPlaylist, false);
            DownloadInteractable = false;
        }

        [UIAction("download-all-click")]
        private void DownloadAllPressed()
        {
            DidPressDownload?.Invoke(selectedPlaylist, true);
            DownloadInteractable = false;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        internal void ShowDetail(IGenericEntry selectedPlaylist)
        {
            this.selectedPlaylist = selectedPlaylist;
            DownloadInteractable = !selectedPlaylist.Owned;
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(PlaylistAuthor));
            NotifyPropertyChanged(nameof(PlaylistDescription));
            playlistCoverView.sprite = selectedPlaylist.Sprite;
            descriptionTextPage.ScrollTo(0, true);
        }

        [UIValue("download-interactable")]
        public bool DownloadInteractable
        {
            get => _downloadInteractable;
            set
            {
                _downloadInteractable = value;
                NotifyPropertyChanged(nameof(DownloadInteractable));
            }
        }
    }
}
