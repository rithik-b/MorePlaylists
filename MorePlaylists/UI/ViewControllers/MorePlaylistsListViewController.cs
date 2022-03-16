using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using BeatSaberMarkupLanguage.Parser;
using MorePlaylists.Sources;
using Zenject;

namespace MorePlaylists.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\MorePlaylistsListView.bsml")]
    [ViewDefinition("MorePlaylists.UI.Views.MorePlaylistsListView.bsml")]
    internal class MorePlaylistsListViewController : BSMLAutomaticViewController, IProgress<float>
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private SpriteLoader spriteLoader;
        private ScrollView scrollView;

        private LoadingControl loadingSpinner;
        private CancellationTokenSource tokenSource;
        private ISource currentSource;
        private List<GenericEntry> currentPlaylists;
        private string currentQuery;
        private float currentScrollPosition;
        private bool exhaustedPlaylists;

        private static SemaphoreSlim listUpdateSemaphore = new SemaphoreSlim(1, 1);

        internal event Action<GenericEntry> DidSelectPlaylist;
        internal event Action DidClickSource;
        internal event Action DidClickSearch;

        [UIComponent("list")]
        private readonly CustomListTableData customListTableData;

        [UIComponent("loading-modal")]
        public RectTransform loadingModal;

        [UIParams]
        internal BSMLParserParams parserParams;

        [Inject]
        public void Construct(List<ISource> sources, StandardLevelDetailViewController standardLevelDetailViewController, SpriteLoader spriteLoader)
        {
            currentSource = sources.FirstOrDefault();
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.spriteLoader = spriteLoader;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            ShowPlaylists();
            scrollView.scrollPositionChangedEvent += OnScrollPositionChanged;
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            scrollView.scrollPositionChangedEvent -= OnScrollPositionChanged;
        }

        #region Actions

        [UIAction("#post-parse")]
        private void PostParse()
        {
            loadingSpinner = GameObject.Instantiate(Accessors.LoadingControlAccessor(ref standardLevelDetailViewController), loadingModal);
            Destroy(loadingSpinner.GetComponent<Touchable>());
            scrollView = Accessors.ScrollViewAccessor(ref customListTableData.tableView);
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

        [UIAction("abort-click")]
        internal void AbortLoading()
        {
            tokenSource?.Cancel();
            SetLoading(false);
        }

        [UIAction("search-click")]
        private void SearchClick() => DidClickSearch?.Invoke();

        #endregion

        internal void Search(string query) => ShowPlaylists(true, query);

        internal void SetEntryAsOwned(IGenericEntry playlistEntry)
        {
            var index = currentPlaylists.IndexOf(playlistEntry);
            if (index >= 0)
            {
                customListTableData.data[index] = new CustomListTableData.CustomCellInfo($"<#7F7F7F>{playlistEntry.Title}", playlistEntry.Author);
                spriteLoader.GetSpriteForEntry(playlistEntry, (Sprite sprite) =>
                {
                    customListTableData.data[index].icon = sprite;
                    customListTableData.tableView.ReloadDataKeepingPosition();
                });
            }
        }

        #region Show Playlists

        internal void ShowPlaylistsForSource(ISource source)
        {
            currentSource = source;
            ShowPlaylists(false);
        }

        private async void ShowPlaylists(bool refreshRequested = false, string query = null)
        {
            await listUpdateSemaphore.WaitAsync();
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            tokenSource = new CancellationTokenSource();
            SetLoading(true);

            currentQuery = query;
            currentPlaylists = await currentSource.GetEndpointResult(refreshRequested, true, this, tokenSource.Token, query);

            PlaylistLibUtils.UpdatePlaylistsOwned(currentPlaylists.Cast<IGenericEntry>().ToList());
            SetLoading(true, 100);

            if (currentPlaylists != null)
            {
                foreach (var playlistEntry in currentPlaylists)
                {
                    var customCellInfo = new CustomListTableData.CustomCellInfo(playlistEntry.DownloadBlocked ? $"<#7F7F7F>{playlistEntry.Title}" : playlistEntry.Title,
                        playlistEntry.Author);
                    customListTableData.data.Add(customCellInfo);
                    spriteLoader.GetSpriteForEntry(playlistEntry, (Sprite sprite) =>
                    {
                        customCellInfo.icon = sprite;
                        customListTableData.tableView.ReloadDataKeepingPosition();
                    });
                }
            }
            customListTableData.tableView.ReloadData();
            SetLoading(false);
            listUpdateSemaphore.Release();
        }

        private async void OnScrollPositionChanged(float newPos)
        {
            if (!currentSource.PagingSupport || listUpdateSemaphore.CurrentCount == 0 || exhaustedPlaylists || currentScrollPosition == newPos)
            {
                return;
            }

            currentScrollPosition = newPos;
            scrollView.RefreshButtons();

            if (!Accessors.PageDownAccessor(ref scrollView).interactable)
            {
                await listUpdateSemaphore.WaitAsync();
                customListTableData.tableView.ClearSelection();
                tokenSource = new CancellationTokenSource();
                SetLoading(true);

                var playlistsToAdd = await currentSource.GetEndpointResult(false, false, this, tokenSource.Token, currentQuery);

                // If we get an empty result, we can't scroll anymore
                if (playlistsToAdd.Count == 0)
                {
                    exhaustedPlaylists = true;
                }

                currentPlaylists.AddRange(playlistsToAdd);

                PlaylistLibUtils.UpdatePlaylistsOwned(playlistsToAdd.Cast<IGenericEntry>().ToList());
                SetLoading(true, 100);

                if (currentPlaylists != null)
                {
                    foreach (var playlistEntry in playlistsToAdd)
                    {
                        var customCellInfo = new CustomListTableData.CustomCellInfo(playlistEntry.DownloadBlocked ? $"<#7F7F7F>{playlistEntry.Title}" : playlistEntry.Title,
                            playlistEntry.Author);
                        customListTableData.data.Add(customCellInfo);
                        spriteLoader.GetSpriteForEntry(playlistEntry, (Sprite sprite) =>
                        {
                            customCellInfo.icon = sprite;
                            customListTableData.tableView.ReloadDataKeepingPosition();
                        });
                    }
                }
                customListTableData.tableView.ReloadDataKeepingPosition();
                SetLoading(false);
                listUpdateSemaphore.Release();
            }
        }

        #endregion

        private void SetLoading(bool value, float progress = 0)
        {
            if (value && isActiveAndEnabled)
            {
                parserParams.EmitEvent("open-loading-modal");
                loadingSpinner.ShowDownloadingProgress("Fetching More Playlists... ", progress);
            }
            else
            {
                parserParams.EmitEvent("close-loading-modal");
            }
        }

        public void Report(float progress)
        {
            loadingSpinner.ShowDownloadingProgress("Fetching More Playlists... ", progress);
        }
    }
}
