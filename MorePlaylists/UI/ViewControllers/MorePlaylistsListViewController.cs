﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;
using UnityEngine;
using BeatSaberMarkupLanguage.Parser;
using System.ComponentModel;
using MorePlaylists.Sources;
using Zenject;

namespace MorePlaylists.UI
{
    public class MorePlaylistsListViewController : BSMLResourceViewController, INotifyPropertyChanged
    {
        private LoadingControl loadingSpinner;
        private CancellationTokenSource tokenSource;
        private static SemaphoreSlim listUpdateSemaphore = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim imageLoadSemaphore = new SemaphoreSlim(1, 1);
        private ISource currentSource;
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

        [Inject]
        public void Construct(List<ISource> sources)
        {
            currentSource = sources.FirstOrDefault();
        }

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
        internal void AbortLoading()
        {
            tokenSource?.Cancel();
            SetLoading(false);
        }

        [UIAction("search-click")]
        private void SearchClick() => DidClickSearch?.Invoke();

        internal async void Search(string query)
        {
            await listUpdateSemaphore.WaitAsync();
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            tokenSource = new CancellationTokenSource();
            SetLoading(true);

            currentPlaylists = await currentSource.GetEndpointResultTask(false, tokenSource.Token);
            currentPlaylists = currentPlaylists.Where(e => e.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 || 
                e.Author.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 || e.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            SetLoading(true, 100);

            foreach (GenericEntry playlist in currentPlaylists)
            {
                if (playlist.DownloadBlocked)
                {
                    customListTableData.data.Add(new CustomCellInfo($"<#7F7F7F>{playlist.Title}", playlist.Author, playlist.Sprite));
                }
                else
                {
                    customListTableData.data.Add(new CustomCellInfo(playlist.Title, playlist.Author, playlist.Sprite));
                }
            }

            customListTableData.tableView.ReloadData();
            SetLoading(false);
            listUpdateSemaphore.Release();
        }

        internal void ShowPlaylistsForSource(ISource source)
        {
            currentSource = source;
            ShowPlaylists(false);
        }

        internal void SetEntryAsOwned(IGenericEntry playlistEntry)
        {
            int index = currentPlaylists.IndexOf(playlistEntry);
            if (index >= 0)
            {
                customListTableData.data[index] = new CustomCellInfo($"<#7F7F7F>{playlistEntry.Title}", playlistEntry.Author, playlistEntry.Sprite);
                customListTableData.tableView.ReloadDataKeepingPosition();
            }
        }

        private async void ShowPlaylists(bool refreshRequested = false)
        {
            await listUpdateSemaphore.WaitAsync();
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();
            SetLoading(true);

            currentPlaylists = await currentSource.GetEndpointResultTask(refreshRequested, tokenSource.Token);

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
            SetLoading(false);
            listUpdateSemaphore.Release();
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

        private void ShowPlaylist(GenericEntry playlistEntry)
        {
            if (playlistEntry.DownloadBlocked)
            {
                customListTableData.data.Add(new CustomCellInfo($"<#7F7F7F>{playlistEntry.Title}", playlistEntry.Author, playlistEntry.Sprite));
            }
            else
            {
                customListTableData.data.Add(new CustomCellInfo(playlistEntry.Title, playlistEntry.Author, playlistEntry.Sprite));
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
    }
}
