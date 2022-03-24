using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using SiraUtil.Web;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace MorePlaylists.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\MorePlaylistsSongListView.bsml")]
    [ViewDefinition("MorePlaylists.UI.Views.MorePlaylistsSongListView.bsml")]
    internal class MorePlaylistsSongListViewController : BSMLAutomaticViewController
    {
        [Inject]
        private readonly IHttpService siraHttpService = null!;
        
        [Inject]
        private readonly SpriteLoader spriteLoader = null!;
        
        [UIComponent("list")]
        private readonly CustomListTableData? customListTableData = null!;
        
        private IEntry? currentEntry;
        private CancellationTokenSource? cancellationTokenSource;
        
        private ScrollView? scrollView;
        private float? currentScrollPosition;
        
        private readonly SemaphoreSlim songLoadSemaphore = new(1, 1);

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (scrollView != null)
            {
                scrollView.scrollPositionChangedEvent += OnScrollPositionChanged;
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

        public void SetCurrentPlaylist(IEntry entry)
        {
            if (customListTableData == null)
            {
                return;
            }

            currentEntry = entry;
            
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            _ = InitSongList(entry, cancellationTokenSource.Token, true);
        }

        private void OnScrollPositionChanged(float newPos)
        {
            if (scrollView == null || currentEntry == null || currentEntry.ExhaustedPages || currentScrollPosition == newPos || songLoadSemaphore.CurrentCount == 0)
            {
                return;
            }
            
            currentScrollPosition = newPos;
            scrollView.RefreshButtons();

            if (!Accessors.PageDownAccessor(ref scrollView).interactable)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
                _ = InitSongList(currentEntry, cancellationTokenSource.Token, false);
            }
        }
        
        [UIAction("#post-parse")]
        private void PostParse() => scrollView = Accessors.ScrollViewAccessor(ref customListTableData!.tableView);

        [UIAction("list-select")]
        private void Select(TableView _, int __) => customListTableData!.tableView.ClearSelection();

        private async Task InitSongList(IEntry entry, CancellationToken cancellationToken, bool firstPage)
        {
            if (customListTableData == null)
            {
                return;
            }

            await songLoadSemaphore.WaitAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            try
            {
                if (firstPage)
                {
                    await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        customListTableData.data.Clear();
                        customListTableData.tableView.ReloadData();
                        Loaded = false;
                    });   
                }

                // We check the cancellationtoken at each interval instead of running everything with a single token
                // due to unity not liking it
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            
                var songs = await entry.GetSongs(siraHttpService, cancellationToken, firstPage);
            
                if (cancellationToken.IsCancellationRequested || songs == null)
                {
                    return;
                }
            
                foreach (var song in songs)
                {
                    var customCellInfo = new CustomListTableData.CustomCellInfo(song.Name, song.SubName,
                        BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                
                    _ = spriteLoader.DownloadSpriteAsync(song.CoverURL, sprite =>
                    {
                        customCellInfo.icon = sprite;
                        customListTableData.tableView.ReloadDataKeepingPosition();
                    }, cancellationToken);
                
                    customListTableData.data.Add(customCellInfo);
                                
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
            finally
            {
                Loaded = true;
                await SiraUtil.Extras.Utilities.PauseChamp;
                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    customListTableData.tableView.ReloadDataKeepingPosition();
                });
                songLoadSemaphore.Release();
            }
        }
        
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
