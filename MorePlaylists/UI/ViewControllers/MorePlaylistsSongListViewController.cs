using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using MorePlaylists.Entries;
using SongDetailsCache;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MorePlaylists.UI
{
    public class MorePlaylistsSongListViewController : BSMLResourceViewController
    {
        private LoadingControl loadingSpinner;
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private IGenericEntry playlistEntry;
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsSongListView.bsml";

        [UIComponent("list")]
        private CustomListTableData customListTableData;

        [UIComponent("loading-modal")]
        public RectTransform loadingModal;

        [UIParams]
        internal BSMLParserParams parserParams;

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

            customListTableData.data.Clear();
            customListTableData.tableView.ReloadData();

            if (playlistEntry.DownloadState == DownloadState.Error)
            {
                SetLoading(false);
            }
            else if (playlistEntry.DownloadState == DownloadState.DownloadedPlaylist)
            {
                InitSongList();
            }
            else
            {
                playlistEntry.FinishedDownload += InitSongList;
            }
        }

        [UIAction("list-select")]
        private void Select(TableView _, int __)
        {
            customListTableData.tableView.ClearSelection();
        }

        private async void InitSongList()
        {
            await semaphoreSlim.WaitAsync();
            if (customListTableData.data.Count == 0)
            {
                if (playlistEntry.Playlist is LegacyPlaylist playlist)
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
            semaphoreSlim.Release();
            SetLoading(false);
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
                loadingSpinner.ShowDownloadingProgress(details, (float)progress);
            }
            else
            {
                parserParams.EmitEvent("close-loading-modal");
            }
        }
    }
}
