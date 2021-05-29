using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using MorePlaylists.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace MorePlaylists.UI
{
    public class MorePlaylistsListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsListView.bsml";

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation)
            {
                InitPlaylistList();
            }
        }

        [UIAction("#post-parse")]
        internal void SetupList()
        {
            InitPlaylistList();
        }

        private async void InitPlaylistList()
        {
            customListTableData.data.Clear();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            List<LegacyPlaylist> playlists = await BSaberUtils.GetEndpointResultTask(false, tokenSource.Token);

            if (playlists != null)
            {
                foreach (LegacyPlaylist playlist in playlists)
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
