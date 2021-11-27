using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using SongDetailsCache;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\MorePlaylistsSongListView.bsml")]
    [ViewDefinition("MorePlaylists.UI.Views.MorePlaylistsSongListView.bsml")]
    internal class MorePlaylistsSongListViewController : BSMLAutomaticViewController
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private SpriteLoader spriteLoader;

        private LoadingControl loadingSpinner;
        private IGenericEntry playlistEntry;
        private static SemaphoreSlim songLoadSemaphore = new SemaphoreSlim(1, 1);

        [UIComponent("list")]
        private readonly CustomListTableData customListTableData;

        [UIComponent("loading-modal")]
        public RectTransform loadingModal;

        [UIParams]
        internal BSMLParserParams parserParams;

        [Inject]
        public void Construct(StandardLevelDetailViewController standardLevelDetailViewController, SpriteLoader spriteLoader)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.spriteLoader = spriteLoader;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation)
            {
                SetLoading(false);
                loadingModal.gameObject.SetActive(false);
                ClearList();
            }
        }

        internal void SetCurrentPlaylist(IGenericEntry playlistEntry)
        {
            if (customListTableData == null)
            {
                return;
            }

            SetLoading(true, 0);

            if (this.playlistEntry != null)
            {
                this.playlistEntry.FinishedDownload -= InitSongList;
            }
            this.playlistEntry = playlistEntry;

            ClearList();

            if (playlistEntry.DownloadState == DownloadState.None)
            {
                SetLoading(false);
            }
            else if (playlistEntry.DownloadState == DownloadState.Downloaded)
            {
                InitSongList(playlistEntry);
            }
            else
            {
                playlistEntry.FinishedDownload += InitSongList;
            }
        }

        internal void ClearList()
        {
            customListTableData.data.Clear();
            customListTableData.tableView.ReloadData();
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            loadingSpinner = GameObject.Instantiate(Accessors.LoadingControlAccessor(ref standardLevelDetailViewController), loadingModal);
            Destroy(loadingSpinner.GetComponent<Touchable>());
        }

        [UIAction("list-select")]
        private void Select(TableView _, int __)
        {
            customListTableData.tableView.ClearSelection();
        }

        private async void InitSongList(IGenericEntry playlistEntry)
        {
            await songLoadSemaphore.WaitAsync();
            ClearList();
            SetLoading(true, 0);

            if (customListTableData.data.Count == 0)
            {
                if (playlistEntry.RemotePlaylist is LegacyPlaylist playlist)
                {
                    SongDetails songDetails = await SongDetails.Init();
                    List<IPlaylistSong> playlistSongs = playlist.Distinct(IPlaylistSongComparer<LegacyPlaylistSong>.Default).ToList();
                    SetLoading(true, 100);
                    for (int i = 0; i < playlistSongs.Count; i++)
                    {
                        if (songDetails.songs.FindByHash(playlistSongs[i].Hash, out SongDetailsCache.Structs.Song song))
                        {
                            CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo(song.songName, $"{song.songAuthorName} [{song.levelAuthorName}]", BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                            spriteLoader.DownloadSpriteAsync(song.coverURL, (Sprite sprite) =>
                            {
                                customCellInfo.icon = sprite;
                                customListTableData.tableView.ReloadDataKeepingPosition();
                            });
                            customListTableData.data.Add(customCellInfo);
                        }
                    }
                }
                customListTableData.tableView.ReloadData();
            }
            // I am sorry
            await Task.Delay(500);
            SetLoading(false);
            songLoadSemaphore.Release();
        }

        internal void SetLoading(bool value, double progress = 0, string details = "Loading Songs")
        {
            if (value && isActiveAndEnabled)
            {
                parserParams.EmitEvent("open-loading-modal");
                loadingSpinner.ShowDownloadingProgress(details, (float)progress);
            }
            else
            {
                parserParams.EmitEvent("close-loading-modal");
            }
        }
    }
}
