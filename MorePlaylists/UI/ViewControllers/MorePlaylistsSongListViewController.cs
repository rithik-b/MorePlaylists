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
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    public class MorePlaylistsSongListViewController : BSMLResourceViewController
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private LoadingControl loadingSpinner;
        private static SemaphoreSlim songLoadSemaphore = new SemaphoreSlim(1, 1);
        private IGenericEntry playlistEntry;
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsSongListView.bsml";

        [UIComponent("list")]
        private CustomListTableData customListTableData;

        [UIComponent("loading-modal")]
        public RectTransform loadingModal;

        [UIParams]
        internal BSMLParserParams parserParams;

        [Inject]
        public void Construct(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation)
            {
                ClearList();
            }
        }

        internal void SetCurrentPlaylist(IGenericEntry playlistEntry)
        {
            if (customListTableData == null)
            {
                return;
            }

            SetLoading(true);

            if (this.playlistEntry != null)
            {
                this.playlistEntry.FinishedDownload -= InitSongList;
            }
            this.playlistEntry = playlistEntry;

            ClearList();

            if (playlistEntry.DownloadState == DownloadState.Error)
            {
                SetLoading(false);
            }
            else if (playlistEntry.DownloadState == DownloadState.DownloadedPlaylist || playlistEntry.DownloadState == DownloadState.Downloaded)
            {
                InitSongList();
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

        internal void AbortLoading() => SetLoading(false);

        [UIAction("#post-parse")]
        private void PostParse()
        {
            loadingSpinner = GameObject.Instantiate(Utils.LoadingControlAccessor(ref standardLevelDetailViewController), loadingModal);
            Destroy(loadingSpinner.GetComponent<Touchable>());
        }

        [UIAction("list-select")]
        private void Select(TableView _, int __)
        {
            customListTableData.tableView.ClearSelection();
        }

        private async void InitSongList()
        {
            await songLoadSemaphore.WaitAsync();
            if (customListTableData.data.Count == 0)
            {
                if (playlistEntry.RemotePlaylist is LegacyPlaylist playlist)
                {
                    SongDetails songDetails = await SongDetails.Init();
                    SetLoading(true, 100);
                    foreach (LegacyPlaylistSong playlistSong in playlist.Distinct(IPlaylistSongComparer<LegacyPlaylistSong>.Default))
                    {
                        if (songDetails.songs.FindByHash(playlistSong.Hash, out SongDetailsCache.Structs.Song song))
                        {
                            customListTableData.data.Add(new CustomListTableData.CustomCellInfo(song.songName, $"{song.songAuthorName} [{song.levelAuthorName}]"));
                        }
                    }
                }
                customListTableData.tableView.ReloadData();
            }
            songLoadSemaphore.Release();
            SetLoading(false);
        }

        private void SetLoading(bool value, double progress = 0, string details = "")
        {
            if (value)
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
