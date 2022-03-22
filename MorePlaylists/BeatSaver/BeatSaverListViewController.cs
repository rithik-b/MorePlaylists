using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaverSharp;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Sources;
using MorePlaylists.UI;
using MorePlaylists.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MorePlaylists.BeatSaver;

[HotReload(RelativePathToLayout = @"..\UI\Views\BasicListView.bsml")]
[ViewDefinition("MorePlaylists.UI.Views.BasicListView.bsml")]
internal class BeatSaverListViewController : BSMLAutomaticViewController, IListViewController, IDisposable
{
    [Inject]
    private readonly SpriteLoader spriteLoader = null!;
    
    [Inject]
    private readonly InputFieldGrabber inputFieldGrabber = null!;
    
    private readonly SemaphoreSlim playlistLoadSemaphore = new(1, 1);

    private CancellationTokenSource? loadCancellationTokenSource;
    private List<BeatSaverEntry> allPlaylists = new();
    private BeatSaver? currentSource;
    
    private ScrollView? scrollView;
    private float? currentScrollPosition;

    public ViewController ViewController => this;
    public event Action<IEntry>? DidSelectPlaylist;
    public event Action? DidClickSource;

    [UIComponent("source-button")]
    private readonly ButtonIconImage? sourceButton = null!;
    
    [UIComponent("list")]
    private readonly CustomListTableData? customListTableData = null!;

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        if (currentSource != null && customListTableData != null && sourceButton != null && scrollView != null)
        {
            sourceButton.image.sprite = currentSource.Logo;
            
            loadCancellationTokenSource?.Cancel();
            loadCancellationTokenSource?.Dispose();
            loadCancellationTokenSource = new CancellationTokenSource();
            
            scrollView.scrollPositionChangedEvent += OnScrollPositionChanged;
            
            _ = LoadPlaylists(currentSource, loadCancellationTokenSource.Token, true);
        }
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        if (scrollView != null)
        {
            scrollView.scrollPositionChangedEvent -= OnScrollPositionChanged;
        }
    }

    public void Dispose()
    {
        playlistLoadSemaphore.Dispose();
        loadCancellationTokenSource?.Dispose();
        if (filtersButton != null && clearFiltersButton != null)
        {
            filtersButton.onClick.RemoveAllListeners();
            clearFiltersButton.onClick.RemoveAllListeners();
        }
    }

    #region Actions

    [UIAction("#post-parse")]
    private void PostParse()
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.localPosition = Vector3.zero;
        scrollView = Accessors.ScrollViewAccessor(ref customListTableData!.tableView);
        InstantiateFilterBar();
    }

    [UIAction("list-select")]
    private void Select(TableView tableView, int row) => DidSelectPlaylist?.Invoke(allPlaylists![row]);

    [UIAction("source-click")]
    private void DisplaySources() => DidClickSource?.Invoke();
    
    public void AbortLoading()
    {
        loadCancellationTokenSource?.Cancel();
        loadCancellationTokenSource?.Dispose();
        loadCancellationTokenSource = null;
        Loaded = true;
    }

    #endregion
    
    public void SetEntryAsOwned(IEntry playlistEntry)
    {
        var index = allPlaylists.IndexOf(playlistEntry);
        if (index >= 0 && customListTableData != null)
        {
            customListTableData.data[index] = new CustomListTableData.CustomCellInfo($"<#7F7F7F>{playlistEntry.Title}", playlistEntry.Author);
            _ = spriteLoader.DownloadSpriteAsync(playlistEntry.SpriteURL, sprite =>
            {
                customListTableData.data[index].icon = sprite;
                customListTableData.tableView.ReloadDataKeepingPosition();
            });
        }
    }

    #region Show Playlists

    public void ShowPlaylistsForSource(ISource source)
    {
        if (source is BeatSaver beatSaverSource)
        {
            currentSource = beatSaverSource;
            ClearFilters();
        }
    }

    private async Task LoadPlaylists(BeatSaver source, CancellationToken cancellationToken, bool refreshRequested = false)
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
            if (refreshRequested)
            {
                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    customListTableData.tableView.ClearSelection();
                    customListTableData.data.Clear();
                    allPlaylists.Clear();
                    Loaded = false;
                });   
            }

            // We check the cancellationtoken at each interval instead of running everything with a single token
            // due to unity not liking it
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var currentPlaylists = await source.GetPage(refreshRequested, cancellationToken);

            if (cancellationToken.IsCancellationRequested || currentPlaylists == null)
            {
                return;
            }
            
            PlaylistLibUtils.UpdatePlaylistsOwned(currentPlaylists.Cast<IEntry>().ToList());
                
            await ShowPlaylists(currentPlaylists, cancellationToken);
        }
        finally
        {
            Loaded = true;
            await SiraUtil.Extras.Utilities.PauseChamp;
            await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
            {
                customListTableData.tableView.ReloadDataKeepingPosition();
            });
            playlistLoadSemaphore.Release();
        }
    }

    private async Task ShowPlaylists(List<BeatSaverEntry> currentPlaylists, CancellationToken cancellationToken)
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
            allPlaylists.Add(playlistEntry);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
    
    private void OnScrollPositionChanged(float newPos)
    {
        if (scrollView == null || currentSource == null || currentSource.ExhaustedPlaylists || playlistLoadSemaphore.CurrentCount == 0 || currentScrollPosition == newPos)
        {
            return;
        }

        currentScrollPosition = newPos;
        scrollView.RefreshButtons();

        if (!Accessors.PageDownAccessor(ref scrollView).interactable)
        {
            loadCancellationTokenSource?.Cancel();
            loadCancellationTokenSource?.Dispose();
            loadCancellationTokenSource = new CancellationTokenSource();
            _ = LoadPlaylists(currentSource, loadCancellationTokenSource.Token);
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

    #region FilterBar
    
    private Button? filtersButton;
    private Button? clearFiltersButton;
    private GameObject? placeholderText;
    private CurvedTextMeshPro? filterText;

    public event Action? FilterViewRequested;
    public event Action? FilterClearRequested;

    [UIComponent("filter-bar")] 
    private readonly RectTransform filterBarTransform = null!;
    
    private void InstantiateFilterBar()
    {
        filtersButton = inputFieldGrabber.GetNewFiltersButton(filterBarTransform);
        if (filtersButton.transform is RectTransform filtersButtonTransform)
        {
            clearFiltersButton = filtersButtonTransform.Find("ClearButton").GetComponent<NoTransitionsButton>();
            placeholderText = filtersButtonTransform.Find("PlaceholderText").gameObject;
            filterText = filtersButtonTransform.Find("Text").GetComponent<CurvedTextMeshPro>();
            filtersButton.onClick.AddListener(FilterClicked);
            clearFiltersButton.onClick.AddListener(ClearFilters);
            
            filtersButtonTransform.SetSiblingIndex(0);
            filtersButtonTransform.sizeDelta = new Vector2(50, 8);
        }

        if (currentSource != null)
        {
            SetActiveFilter(currentSource.CurrentFilters);
        }
    }

    private void FilterClicked() => FilterViewRequested?.Invoke();

    private void ClearFilters()
    {
        FilterClearRequested?.Invoke();

        if (currentSource != null)
        {
            loadCancellationTokenSource?.Cancel();
            loadCancellationTokenSource?.Dispose();
            loadCancellationTokenSource = new CancellationTokenSource();
            _ = LoadPlaylists(currentSource, CancellationToken.None, true);
        }
    }

    public void SetActiveFilter(BeatSaverFilterModel filterOptions)
    {
        switch (filterOptions.FilterMode)
        {
            case FilterMode.Search:
                if (filterOptions.NullableSearchFilter == null)
                {
                    FilterString = string.Empty;
                    return;
                }

                var sb = new StringBuilder();
        
                if (!string.IsNullOrWhiteSpace(filterOptions.SearchFilter.Query))
                {
                    sb.Append(filterOptions.SearchFilter.Query + ", ");
                }

                if (filterOptions.SearchFilter.IncludeEmpty)
                {
                    sb.Append("IncludeEmpty, ");
                }

                if (filterOptions.SearchFilter.IsCurated)
                {
                    sb.Append("CuratedOnly, ");
                }

                if (sb.Length >= 2)
                {
                    sb.Remove(sb.Length - 2, 2);
                }
                
                FilterString = sb.ToString();
                break;
            
            case FilterMode.User:
                FilterString = $"user:{filterOptions.UserName}";
                break;
        }

        if (isActiveAndEnabled && currentSource != null)
        {
            loadCancellationTokenSource?.Cancel();
            loadCancellationTokenSource?.Dispose();
            loadCancellationTokenSource = new CancellationTokenSource();
            _ = LoadPlaylists(currentSource, loadCancellationTokenSource.Token, true);
        }
    }

    private string filterString = string.Empty;
    private string FilterString
    {
        get => filterString;
        set
        {
            filterString = value;

            if (clearFiltersButton == null || placeholderText == null || filterText == null)
            {
                return;
            }
            
            if (string.IsNullOrWhiteSpace(value))
            {
                filterText.gameObject.SetActive(false);
                placeholderText.SetActive(true);
                clearFiltersButton.gameObject.SetActive(false);
            }
            else
            {
                filterText.text = value;
                filterText.gameObject.SetActive(true);
                placeholderText.SetActive(false);
                clearFiltersButton.gameObject.SetActive(true);
            }
        }
    }

    #endregion
}
