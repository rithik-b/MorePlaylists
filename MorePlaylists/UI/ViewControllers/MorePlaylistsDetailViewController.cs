﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using System;
using System.ComponentModel;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    public class MorePlaylistsDetailViewController : BSMLResourceViewController, IInitializable, IDisposable, INotifyPropertyChanged
    {
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsDetailView.bsml";
        private IGenericEntry selectedPlaylist;
        private bool queueFull;

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
            NotifyPropertyChanged(nameof(DownloadInteractable));
        }

        [UIAction("download-all-click")]
        private void DownloadAllPressed()
        {
            DidPressDownload?.Invoke(selectedPlaylist, true);
            NotifyPropertyChanged(nameof(DownloadInteractable));
        }

        public void Initialize()
        {
            MorePlaylistsDownloadQueueViewController.QueueFull += MorePlaylistsDownloadQueueViewController_QueueFull;
            queueFull = false;
        }

        public void Dispose()
        {
            MorePlaylistsDownloadQueueViewController.QueueFull -= MorePlaylistsDownloadQueueViewController_QueueFull;
        }

        private void MorePlaylistsDownloadQueueViewController_QueueFull(bool queueFull)
        {
            this.queueFull = queueFull;
            NotifyPropertyChanged(nameof(DownloadInteractable));
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        internal void ShowDetail(IGenericEntry selectedPlaylist)
        {
            this.selectedPlaylist = selectedPlaylist;
            NotifyPropertyChanged(nameof(DownloadInteractable));
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(PlaylistAuthor));
            NotifyPropertyChanged(nameof(PlaylistDescription));
            playlistCoverView.sprite = selectedPlaylist.Sprite;
            descriptionTextPage.ScrollTo(0, true);
        }

        [UIValue("download-interactable")]
        public bool DownloadInteractable => selectedPlaylist != null && !selectedPlaylist.Owned && !queueFull;
    }
}
