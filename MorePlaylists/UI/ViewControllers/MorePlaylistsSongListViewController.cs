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

        private CancellationTokenSource? cancellationTokenSource;
        
        private bool loaded;
        
        [UIValue("is-loading")]
        public bool IsLoading => !Loaded;

        [UIValue("has-loaded")]
        public bool Loaded
        {
            get => loaded;
            set
            {
                loaded = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation)
            {
                ClearList();
            }
        }

        public void SetCurrentPlaylist(IEntry entry) => _ = InitSongList(entry);

        public void ClearList()
        {
            if (customListTableData != null)
            {
                customListTableData.data.Clear();
                customListTableData.tableView.ReloadData();
            }
        }

        [UIAction("list-select")]
        private void Select(TableView _, int __) => customListTableData!.tableView.ClearSelection();

        private async Task InitSongList(IEntry entry)
        {
            if (customListTableData == null)
            {
                return;
            }
            
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Run(async () =>
                {
                    await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(ClearList);
                    
                    Loaded = false;
                    var songs = await entry.GetSongs(siraHttpService, cancellationTokenSource.Token);

                    if (songs != null)
                    {
                        await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                        {
                            foreach (var song in songs)
                            {
                                var customCellInfo = new CustomListTableData.CustomCellInfo(song.Name, song.SubName,
                                    BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                                _ = spriteLoader.DownloadSpriteAsync(song.CoverURL, sprite =>
                                {
                                    customCellInfo.icon = sprite;
                                    customListTableData.tableView.ReloadDataKeepingPosition();
                                });
                                customListTableData.data.Add(customCellInfo);
                            }
                            customListTableData.tableView.ReloadData();
                        });
                    }

                    await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => Loaded = true);
                }, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException) {}
        }
    }
}
