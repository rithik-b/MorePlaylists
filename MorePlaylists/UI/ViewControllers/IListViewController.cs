using System;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Sources;

namespace MorePlaylists.UI
{
    internal interface IListViewController
    {
        ViewController ViewController { get; }
        event Action<IEntry>? DidSelectPlaylist;
        event Action? DidClickSource;
        event Action? DetailDismissRequested;
        void ShowPlaylistsForSource(ISource source);
        void SetEntryAsOwned(IEntry entry);
        void AbortLoading();
    }
}
