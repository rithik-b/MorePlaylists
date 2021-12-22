using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using SiraUtil.Web;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\MorePlaylistsSongListView.bsml")]
    [ViewDefinition("MorePlaylists.UI.Views.MorePlaylistsSongListView.bsml")]
    internal class MorePlaylistsSongListViewController : BSMLAutomaticViewController
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private IHttpService siraHttpService;
        private SpriteLoader spriteLoader;

        private LoadingControl loadingSpinner;
        private static SemaphoreSlim songLoadSemaphore = new SemaphoreSlim(1, 1);

        [UIComponent("list")]
        private readonly CustomListTableData customListTableData;

        [UIComponent("loading-modal")]
        public RectTransform loadingModal;

        [UIParams]
        internal BSMLParserParams parserParams;

        [Inject]
        public void Construct(StandardLevelDetailViewController standardLevelDetailViewController, IHttpService siraHttpService, SpriteLoader spriteLoader)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.siraHttpService = siraHttpService;
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
            if (customListTableData != null)
            {
                InitSongList(playlistEntry);
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
                List<Song> songs = await playlistEntry.GetSongs(siraHttpService);
                foreach (Song song in songs)
                {
                    CustomListTableData.CustomCellInfo customCellInfo = new CustomListTableData.CustomCellInfo(song.Name, song.SubName, BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                    spriteLoader.DownloadSpriteAsync(song.CoverURL, (Sprite sprite) =>
                    {
                        customCellInfo.icon = sprite;
                        customListTableData.tableView.ReloadDataKeepingPosition();
                    });
                    customListTableData.data.Add(customCellInfo);
                }
                customListTableData.tableView.ReloadData();
            }

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
