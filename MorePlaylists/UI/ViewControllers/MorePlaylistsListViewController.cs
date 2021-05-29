using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Legacy;
using HMUI;
using MorePlaylists.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace MorePlaylists.UI
{
    public class MorePlaylistsListViewController : BSMLResourceViewController
    {
        private List<LegacyPlaylist> currentPlaylists;
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsListView.bsml";

        public Action<LegacyPlaylist> didSelectPlaylist;

        [UIComponent("list")]
        private CustomListTableData customListTableData;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation)
            {
                InitPlaylistList();
            }
        }

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            InitPlaylistList();
        }

        [UIAction("list-select")]
        internal void Select(TableView tableView, int row)
        {
            didSelectPlaylist?.Invoke(currentPlaylists[row]);
        }

        private async void InitPlaylistList()
        {
            customListTableData.tableView.ClearSelection();
            customListTableData.data.Clear();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            currentPlaylists = await BSaberUtils.GetEndpointResultTask(false, tokenSource.Token);

            if (currentPlaylists != null)
            {
                foreach (LegacyPlaylist playlist in currentPlaylists)
                {
                    if (!playlist.SpriteWasLoaded)
                    {
                        _ = playlist.Sprite;
                        playlist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
                        playlist.SpriteLoaded += DeferredSpriteLoadPlaylist_SpriteLoaded;
                    }
                    else
                    {
                        ShowPlaylist(playlist);
                    }
                }
            }
            customListTableData.tableView.ReloadData();
        }

        private void DeferredSpriteLoadPlaylist_SpriteLoaded(object sender, EventArgs e)
        {
            if (sender is LegacyPlaylist playlist)
            {
                ShowPlaylist(playlist);
                customListTableData.tableView.ReloadData();
                playlist.SpriteLoaded -= DeferredSpriteLoadPlaylist_SpriteLoaded;
            }
        }

        private void ShowPlaylist(LegacyPlaylist playlist)
        {
            customListTableData.data.Add(new CustomCellInfo(playlist.Title, playlist.Author, playlist.Sprite));
        }
    }
}
