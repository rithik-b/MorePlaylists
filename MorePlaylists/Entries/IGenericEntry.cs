using System;
using System.IO;
using UnityEngine;

namespace MorePlaylists.Entries
{
    public interface IGenericEntry
    {
        string Title { get; }
        string Author { get; }
        string Description { get; }
        string PlaylistURL { get; }
        BeatSaberPlaylistsLib.Types.IPlaylist RemotePlaylist { get; }
        BeatSaberPlaylistsLib.Types.IPlaylist LocalPlaylist { get; set; }
        event Action FinishedDownload;
        DownloadState DownloadState { get; set; }
        bool DownloadBlocked { get; set; }
        Sprite Sprite { get; }
        Stream GetCoverStream();
    }
}
