using System;
using HMUI;
using MorePlaylists.Entries;

namespace MorePlaylists.UI;

public interface IDetailViewController
{
    ViewController ViewController { get; }
    event Action<IEntry, bool> DidPressDownload;
    event Action<BeatSaberPlaylistsLib.Types.IPlaylist> DidGoToPlaylist;
    void ShowDetail(IEntry entry);
    void OnPlaylistDownloaded();
}
