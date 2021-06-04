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

namespace MorePlaylists.UI
{
    public class MorePlaylistsListViewController : BSMLResourceViewController
    {
        private LoadingControl loadingSpinner;
        private CancellationTokenSource tokenSource;
        private static SemaphoreSlim imageLoadSemaphore = new SemaphoreSlim(1, 1);
        private List<GenericEntry> currentPlaylists;
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsListView.bsml";

        public Action<GenericEntry> didSelectPlaylist;

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
                InitPlaylistList();
            }
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            InitPlaylistList();
        }

        [UIAction("list-select")]
        private void Select(TableView tableView, int row)
        {
            didSelectPlaylist?.Invoke(currentPlaylists[row]);
        }


        [UIAction("refresh-click")]
        private void RefreshList()
        {
            foreach (GenericEntry playlist in currentPlaylists)
            {
                playlist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
            }
            InitPlaylistList(true);
        }

        [UIAction("abort-click")]
        private void AbortLoading()
        {
            tokenSource.Cancel();
            SetLoading(false);
        }

        private async void InitPlaylistList(bool refreshRequested = false)
        {
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();
            SetLoading(true);
            currentPlaylists = (await BSaberUtils.GetEndpointResultTask(refreshRequested, tokenSource.Token)).Cast<GenericEntry>().ToList();
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

        public void SetLoading(bool value, double progress = 0, string details = "")
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
