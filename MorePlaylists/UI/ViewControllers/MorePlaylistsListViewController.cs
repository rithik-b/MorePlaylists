using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Types;
using MorePlaylists.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using UnityEngine;
using BeatSaberMarkupLanguage.Parser;
using System.ComponentModel;

namespace MorePlaylists.UI
{
    public class MorePlaylistsListViewController : BSMLResourceViewController, INotifyPropertyChanged
    {
        private LoadingControl loadingSpinner;
        private CancellationTokenSource tokenSource;
        private static SemaphoreSlim imageLoadSemaphore = new SemaphoreSlim(1, 1);
        private DownloadSource currentSource = DownloadSource.BSaber;
        private List<GenericEntry> currentPlaylists;
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsListView.bsml";

        internal event Action<GenericEntry> DidSelectPlaylist;
        internal event Action DidClickSource;
        internal event Action DidClickSearch;

        [UIComponent("list")]
        private CustomListTableData customListTableData;

        [UIComponent("loading-modal")]
        public RectTransform loadingModal;

        [UIParams]
        internal BSMLParserParams parserParams;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation)
            {
                ShowPlaylists();
            }
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            ShowPlaylists();
        }

        [UIAction("list-select")]
        private void Select(TableView tableView, int row)
        {
            DidSelectPlaylist?.Invoke(currentPlaylists[row]);
        }

        [UIAction("source-click")]
        private void DisplaySources()
        {
            DidClickSource?.Invoke();
        }


        [UIAction("refresh-click")]
        private void RefreshList()
        {
            foreach (GenericEntry playlist in currentPlaylists)
            {
                playlist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
            }
            ShowPlaylists(true);
        }

        [UIAction("abort-click")]
        private void AbortLoading()
        {
            tokenSource.Cancel();
            SetLoading(false);
        }

        [UIAction("search-click")]
        private void SearchClick() => DidClickSearch?.Invoke();

        internal void Search(string query)
        {
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            NotifyPropertyChanged(nameof(ButtonsInteractable));
            currentPlaylists = currentPlaylists.Where(e => e.Title.Contains(query) || e.Author.Contains(query) || e.Description.Contains(query)).ToList();
            foreach (GenericEntry playlist in currentPlaylists)
            {
                if (playlist.Owned)
                {
                    customListTableData.data.Add(new CustomCellInfo($"<#7F7F7F>{playlist.Title}", playlist.Author, playlist.Sprite));
                }
                else
                {
                    customListTableData.data.Add(new CustomCellInfo(playlist.Title, playlist.Author, playlist.Sprite));
                }
            }
            customListTableData.tableView.ReloadData();
            NotifyPropertyChanged(nameof(ButtonsInteractable));
        }

        internal void ShowPlaylistsForSource(DownloadSource downloadSource)
        {
            currentSource = downloadSource;
            ShowPlaylists(false);
        }

        private async void ShowPlaylists(bool refreshRequested = false)
        {
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            NotifyPropertyChanged(nameof(ButtonsInteractable));
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();
            SetLoading(true);

            switch (currentSource)
            {
                case DownloadSource.BSaber:
                    currentPlaylists = (await BSaberUtils.GetEndpointResultTask(refreshRequested, tokenSource.Token)).Cast<GenericEntry>().ToList();
                    break;
                case DownloadSource.Hitbloq:
                    currentPlaylists = (await HitbloqUtils.GetEndpointResultTask(refreshRequested, tokenSource.Token)).Cast<GenericEntry>().ToList();
                    break;
            }

            PlaylistLibUtils.UpdatePlaylistsOwned(currentPlaylists.Cast<IGenericEntry>().ToList());
            SetLoading(true, 100);

            if (currentPlaylists != null)
            {
                foreach (GenericEntry playlist in currentPlaylists)
                {
                    if (!playlist.SpriteWasLoaded)
                    {
                        await imageLoadSemaphore.WaitAsync();
                        _ = playlist.Sprite;
                        playlist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
                        playlist.SpriteLoaded += DeferredSpriteLoadPlaylist_SpriteLoaded;
                    }
                    else
                    {
                        ShowPlaylist(playlist);
                    }
                }
            }
            customListTableData.tableView.ReloadData();
            NotifyPropertyChanged(nameof(ButtonsInteractable));
            SetLoading(false);
        }

        private void DeferredSpriteLoadPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is GenericEntry playlist)
            {
                ShowPlaylist(playlist);
                customListTableData.tableView.ReloadData();
                playlist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
                imageLoadSemaphore.Release();
            }
        }

        private void ShowPlaylist(GenericEntry playlist)
        {
            if (playlist.Owned)
            {
                customListTableData.data.Add(new CustomCellInfo($"<#7F7F7F>{playlist.Title}", playlist.Author, playlist.Sprite));
            }
            else
            {
                customListTableData.data.Add(new CustomCellInfo(playlist.Title, playlist.Author, playlist.Sprite));
            }
        }

        private void SetLoading(bool value, double progress = 0, string details = "")
        {
            if (loadingSpinner == null)
            {
                loadingSpinner = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<LoadingControl>().First(), loadingModal);
                Destroy(loadingSpinner.GetComponent<Touchable>());
            }
            if (value)
            {
                parserParams.EmitEvent("open-loading-modal");
                loadingSpinner.ShowDownloadingProgress("Fetching More Playlists... " + details, (float)progress);
            }
            else
            {
                parserParams.EmitEvent("close-loading-modal");
            }
        }

        [UIValue("buttons-interactable")]
        private bool ButtonsInteractable => customListTableData?.data.Count != 0;
    }
}
