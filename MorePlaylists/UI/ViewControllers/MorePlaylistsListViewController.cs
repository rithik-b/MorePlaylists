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
using System.Threading.Tasks;
using UnityEngine;
using BeatSaberMarkupLanguage.Parser;
using MorePlaylists.Sources;
using Zenject;

namespace MorePlaylists.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\MorePlaylistsListView.bsml")]
    [ViewDefinition("MorePlaylists.UI.Views.MorePlaylistsListView.bsml")]
    internal class MorePlaylistsListViewController : BSMLAutomaticViewController, IListViewController, IProgress<float>
    {
        [Inject]
        private StandardLevelDetailViewController standardLevelDetailViewController = null!;
        
        [Inject]
        private readonly SpriteLoader spriteLoader = null!;
        
        private readonly SemaphoreSlim playlistLoadSemaphore = new(1, 1);

        private LoadingControl? loadingSpinner;
        private CancellationTokenSource? cancellationTokenSource;
        private List<IBasicEntry>? currentPlaylists;
        private IBasicSource? currentSource;
        
        public ViewController ViewController => this;
        public event Action<IEntry>? DidSelectPlaylist;
        public event Action? DidClickSource;
        
        [UIComponent("list")]
        private readonly CustomListTableData? customListTableData = null!;

        [UIComponent("loading-modal")]
        private readonly RectTransform? loadingModal = null!;

        [UIParams] 
        private readonly BSMLParserParams parserParams = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (currentSource != null && customListTableData != null)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                _ = ShowPlaylists(currentSource, cancellationTokenSource.Token);
            }
        }

        #region Actions

        [UIAction("#post-parse")]
        private void PostParse()
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.localPosition = Vector3.zero;
            loadingSpinner = Instantiate(Accessors.LoadingControlAccessor(ref standardLevelDetailViewController), loadingModal);
            Destroy(loadingSpinner.GetComponent<Touchable>());
        }

        [UIAction("list-select")]
        private void Select(TableView tableView, int row) => DidSelectPlaylist?.Invoke(currentPlaylists![row]);

        [UIAction("source-click")]
        private void DisplaySources() => DidClickSource?.Invoke();
        
        [UIAction("abort-click")]
        public void AbortLoading()
        {
            cancellationTokenSource?.Cancel();
            SetLoading(false);
        }

        #endregion
        
        public void SetEntryAsOwned(IEntry playlistEntry)
        {
            var index = currentPlaylists.IndexOf(playlistEntry);
            if (index >= 0 && customListTableData != null)
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

        public void ShowPlaylistsForSource(ISource source)
        {
            if (source is IBasicSource basicSource)
            {
                currentSource = basicSource;
            }
        }

        private async Task ShowPlaylists(IBasicSource source, CancellationToken cancellationToken, bool refreshRequested = false)
        {
            if (customListTableData == null)
            {
                return;
            }
            
            await playlistLoadSemaphore.WaitAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    customListTableData.tableView.ClearSelection();
                    customListTableData.data.Clear();
                    SetLoading(true);
                });

                // We check the cancellationtoken at each interval instead of running everything with a single token
                // due to unity not liking it
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                currentPlaylists = await source.GetEndpointResult(refreshRequested, this, cancellationToken);

                if (cancellationToken.IsCancellationRequested || currentPlaylists == null)
                {
                    return;
                }

                PlaylistLibUtils.UpdatePlaylistsOwned(currentPlaylists.Cast<IEntry>().ToList());

                foreach (var playlistEntry in currentPlaylists)
                {
                    var customCellInfo = new CustomListTableData.CustomCellInfo(playlistEntry.DownloadBlocked
                            ? $"<#7F7F7F>{playlistEntry.Title}"
                            : playlistEntry.Title,
                        playlistEntry.Author);

                    await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        _ = spriteLoader.DownloadSpriteAsync(playlistEntry.SpriteURL, sprite =>
                        {
                            customCellInfo.icon = sprite;
                            customListTableData.tableView.ReloadDataKeepingPosition();
                        }, cancellationToken);
                    });

                    customListTableData.data.Add(customCellInfo);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }

                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    customListTableData.tableView.ReloadData();
                    SetLoading(false);
                });
            }
            finally
            {
                playlistLoadSemaphore.Release();
            }
        }

        #endregion

        private void SetLoading(bool value, float progress = 0)
        {
            if (value && isActiveAndEnabled && loadingSpinner != null)
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
            if (loadingSpinner != null)
            {
                loadingSpinner.ShowDownloadingProgress("Fetching More Playlists... ", progress);
            }
        }
    }
}
