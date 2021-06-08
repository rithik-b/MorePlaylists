using System;
using System.IO;
using UnityEngine;

namespace MorePlaylists.Types
{
    public interface IGenericEntry
    {
        string Title { get; }
        string Author { get; }
        string Description { get; }
        string PlaylistURL { get; }
        BeatSaberPlaylistsLib.Types.IPlaylist Playlist { get; set; }
        event Action FinishedDownload;
        DownloadState DownloadState { get; set; }
        bool Owned { get; set; }
        Sprite Sprite { get; }
        Stream GetCoverStream();
    }
}
