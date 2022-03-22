using System;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Sources;

namespace MorePlaylists.UI;

internal interface IListViewController
{
    ViewController ViewController { get; }
    event Action<IEntry>? DidSelectPlaylist;
    event Action? DidClickSource;
    void ShowPlaylistsForSource(ISource source);
    void SetEntryAsOwned(IEntry entry);
    void AbortLoading();
}
