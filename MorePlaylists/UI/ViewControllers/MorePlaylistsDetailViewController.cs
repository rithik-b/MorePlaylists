using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Legacy;
using HMUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MorePlaylists.UI
{
    public class MorePlaylistsDetailViewController : BSMLResourceViewController, INotifyPropertyChanged
    {
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsDetailView.bsml";
        private LegacyPlaylist selectedPlaylist;

        private bool _downloadInteractable = false;
        private bool _previewInteractable = false;

        [UIComponent("playlist-cover")]
        private readonly Image playlistCoverView;

        [UIComponent("text-page")]
        private readonly TextPageScrollView descriptionTextPage;

        [UIValue("downloadInteractable")]
        public bool DownloadInteractable
        {
            get => _downloadInteractable;
            set
            {
                _downloadInteractable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("previewInteractable")]
        public bool PreviewInteractible
        {
            get => _previewInteractable;
            set
            {
                _previewInteractable = value;
                NotifyPropertyChanged();
            }
        }

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

        internal void ShowDetail(LegacyPlaylist selectedPlaylist)
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
