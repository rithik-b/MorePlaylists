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
    internal class MorePlaylistsListViewController : BSMLAutomaticViewController, IListViewController, IDisposable
    {
        [Inject]
        private readonly SpriteLoader spriteLoader = null!;
        
        [Inject]
        private readonly InputFieldGrabber inputFieldGrabber = null!;
        
        private readonly SemaphoreSlim playlistLoadSemaphore = new(1, 1);

        private InputFieldView? inputFieldView;
        private CancellationTokenSource? loadCancellationTokenSource;
        private CancellationTokenSource? searchCancellationTokenSource;
        private readonly List<IBasicEntry> currentPlaylists = new();
        private List<IBasicEntry>? allPlaylists;
        private IBasicSource? currentSource;
        
        public ViewController ViewController => this;
        public event Action<IEntry>? DidSelectPlaylist;
        public event Action? DidClickSource;

        [UIComponent("filter-bar")] 
        private readonly RectTransform filterBarTransform = null!;
        
        [UIComponent("source-button")]
        private readonly ButtonIconImage? sourceButton = null!;
        
        [UIComponent("list")]
        private readonly CustomListTableData? customListTableData = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (currentSource != null && customListTableData != null && sourceButton != null && inputFieldView != null)
            {
                sourceButton.image.sprite = currentSource.Logo;
                inputFieldView.ClearInput();
                loadCancellationTokenSource?.Cancel();
                loadCancellationTokenSource?.Dispose();
                loadCancellationTokenSource = new CancellationTokenSource();
                _ = LoadPlaylists(currentSource, loadCancellationTokenSource.Token);
            }
        }
        
        public void Dispose()
        {
            playlistLoadSemaphore.Dispose();
            loadCancellationTokenSource?.Dispose();
            searchCancellationTokenSource?.Dispose();
            if (inputFieldView != null)
            {
                inputFieldView.onValueChanged.RemoveAllListeners();
            }
        }

        #region Actions

        [UIAction("#post-parse")]
        private void PostParse()
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.localPosition = Vector3.zero;
            
            inputFieldView = inputFieldGrabber.GetNewInputField(filterBarTransform);
            if (inputFieldView.transform is RectTransform inputFieldTransform)
            {
                inputFieldView.transform.SetSiblingIndex(0);
                inputFieldTransform.sizeDelta = new Vector2(50, 8);
            }
            inputFieldView.onValueChanged.AddListener(inputFieldView =>
            {
                searchCancellationTokenSource?.Cancel();
                searchCancellationTokenSource?.Dispose();
                searchCancellationTokenSource = new CancellationTokenSource();
                _ = SearchAsync(searchCancellationTokenSource.Token);
            });
        }

        [UIAction("list-select")]
        private void Select(TableView tableView, int row) => DidSelectPlaylist?.Invoke(currentPlaylists![row]);

        [UIAction("source-click")]
        private void DisplaySources() => DidClickSource?.Invoke();
        
        public void AbortLoading()
        {
            loadCancellationTokenSource?.Cancel();
            loadCancellationTokenSource?.Dispose();
            searchCancellationTokenSource?.Cancel();
            searchCancellationTokenSource?.Dispose();
            Loaded = true;
        }

        #endregion
        
        public void SetEntryAsOwned(IEntry playlistEntry)
        {
            var index = currentPlaylists.IndexOf(playlistEntry);
            if (index >= 0 && customListTableData != null)
            {
                customListTableData.data[index] = new CustomListTableData.CustomCellInfo($"<#7F7F7F>{playlistEntry.Title}", playlistEntry.Author);
                spriteLoader.GetSpriteForEntry(playlistEntry, sprite =>
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

        private async Task LoadPlaylists(IBasicSource source, CancellationToken cancellationToken, bool refreshRequested = false)
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
                    Loaded = false;
                });

                // We check the cancellationtoken at each interval instead of running everything with a single token
                // due to unity not liking it
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                allPlaylists = await source.GetEndpointResult(refreshRequested, cancellationToken);

                if (cancellationToken.IsCancellationRequested || allPlaylists == null)
                {
                    return;
                }
                
                currentPlaylists.Clear();
                currentPlaylists.AddRange(allPlaylists);
                PlaylistLibUtils.UpdatePlaylistsOwned(currentPlaylists.Cast<IEntry>().ToList());
                
                await ShowPlaylists(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    Loaded = true;
                    customListTableData.tableView.ReloadData();
                });
            }
            finally
            {
                playlistLoadSemaphore.Release();
            }
        }

        private async Task ShowPlaylists(CancellationToken cancellationToken)
        {
            if (customListTableData == null)
            {
                return;
            }
            
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
        }

        private async Task SearchAsync(CancellationToken cancellationToken)
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
                if (allPlaylists == null)
                {
                    return;
                }

                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    customListTableData.tableView.ClearSelection();
                    customListTableData.data.Clear();
                    Loaded = false;
                });
                
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                var searchQuery = inputFieldView != null ? inputFieldView.text : "";
                currentPlaylists.Clear();
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    currentPlaylists.AddRange(allPlaylists);
                }
                else
                {
                    await Task.Run(() =>
                    {
                        foreach (var playlist in allPlaylists)
                        {
                            if (playlist.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                playlist.Author.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                playlist.Description.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                currentPlaylists.Add(playlist);
                            }
                        }
                    }, cancellationToken);
                }
                
                await ShowPlaylists(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    Loaded = true;
                    customListTableData.tableView.ReloadData();
                });
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                playlistLoadSemaphore.Release();
            }
        }

        #endregion

        #region Loading

        private bool loaded;
        [UIValue("is-loading")]
        private bool IsLoading => !Loaded;

        [UIValue("has-loaded")]
        private bool Loaded
        {
            get => loaded;
            set
            {
                loaded = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }

        #endregion
    }
}
