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
        private IHttpService siraHttpService;
        private SpriteLoader spriteLoader;

        private static SemaphoreSlim songLoadSemaphore = new SemaphoreSlim(1, 1);

        private bool _loaded;

        [UIValue("is-loading")]
        public bool IsLoading => !Loaded;

        [UIValue("loaded")]
        public bool Loaded
        {
            get => _loaded;
            set
            {
                _loaded = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }

        [UIComponent("list")]
        private readonly CustomListTableData customListTableData;

        [UIParams]
        internal BSMLParserParams parserParams;

        [Inject]
        public void Construct(IHttpService siraHttpService, SpriteLoader spriteLoader)
        {
            this.siraHttpService = siraHttpService;
            this.spriteLoader = spriteLoader;
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
            if (customListTableData != null)
            {
                InitSongList(playlistEntry);
            }
        }

        internal void ClearList()
        {
            if (customListTableData != null)
            {
                customListTableData.data.Clear();
                customListTableData.tableView.ReloadData();
            }
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
            Loaded = false;

            if (customListTableData.data.Count == 0)
            {
                var songs = await playlistEntry.GetSongs(siraHttpService);
                foreach (var song in songs)
                {
                    var customCellInfo = new CustomListTableData.CustomCellInfo(song.Name, song.SubName, BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                    spriteLoader.DownloadSpriteAsync(song.CoverURL, (Sprite sprite) =>
                    {
                        customCellInfo.icon = sprite;
                        customListTableData.tableView.ReloadDataKeepingPosition();
                    });
                    customListTableData.data.Add(customCellInfo);
                }
                customListTableData.tableView.ReloadData();
            }

            Loaded = true;
            songLoadSemaphore.Release();
        }
    }
}
