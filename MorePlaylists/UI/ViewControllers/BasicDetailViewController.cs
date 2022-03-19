using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using System;
using System.Linq;
using BeatSaberPlaylistsLib.Types;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\BasicDetailView.bsml")]
    [ViewDefinition("MorePlaylists.UI.Views.BasicDetailView.bsml")]
    internal class BasicDetailViewController : BSMLAutomaticViewController, IDetailViewController
    {
        [Inject]
        private readonly SpriteLoader spriteLoader = null!;
        
        private IEntry? selectedPlaylistEntry;
        public ViewController ViewController => this;
        public event Action<IEntry, bool>? DidPressDownload;
        public event Action<IPlaylist>? DidGoToPlaylist;

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView = null!;

        [UIComponent("text-page")]
        private readonly TextPageScrollView descriptionTextPage = null!;

        #region Actions

        [UIAction("#post-parse")]
        private void PostParse()
        {
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            playlistCoverView.material = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UINoGlowRoundEdge");
        }

        [UIAction("download-click")]
        private void DownloadPressed()
        {
            if (selectedPlaylistEntry != null)
            {
                DidPressDownload?.Invoke(selectedPlaylistEntry, false);
                NotifyPropertyChanged(nameof(DownloadInteractable));
            }
        }

        [UIAction("download-all-click")]
        private void DownloadAllPressed()
        {
            if (selectedPlaylistEntry != null)
            {
                DidPressDownload?.Invoke(selectedPlaylistEntry, true);
                NotifyPropertyChanged(nameof(DownloadInteractable));
            }
        }

        [UIAction("go-to-playlist")]
        private void GoToPlaylist()
        {
            if (selectedPlaylistEntry?.LocalPlaylist != null)
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
            spriteLoader.GetSpriteForEntry(selectedPlaylistEntry, sprite => playlistCoverView.sprite = sprite);
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
        public string PlaylistName
        {
            get
            {
                if (selectedPlaylistEntry != null)
                {
                    if (selectedPlaylistEntry.Title.Length > 32)
                    {
                        return selectedPlaylistEntry.Title.Substring(0, 28) + "...";
                    }
                    return selectedPlaylistEntry.Title;
                }
                return string.Empty;
            }
        }

        [UIValue("playlist-author")]
        public string PlaylistAuthor
        {
            get
            {
                if (selectedPlaylistEntry != null)
                {
                    if (selectedPlaylistEntry.Author.Length > 32)
                    {
                        return selectedPlaylistEntry.Author.Substring(0, 28) + "...";
                    }
                    return selectedPlaylistEntry.Author;
                }
                return string.Empty;
            }
        }

        [UIValue("playlist-description")]
        private string PlaylistDescription => string.IsNullOrWhiteSpace(selectedPlaylistEntry?.Description) ? "No Description available for this playlist." : selectedPlaylistEntry?.Description ?? "";

        [UIValue("download-interactable")]
        public bool DownloadInteractable => selectedPlaylistEntry is {DownloadBlocked: false};

        [UIValue("download-active")]
        public bool DownloadActive => selectedPlaylistEntry is {LocalPlaylist: null};

        [UIValue("go-to-active")]
        public bool GoToActive => selectedPlaylistEntry is {LocalPlaylist: { }};

        #endregion
    }
}
