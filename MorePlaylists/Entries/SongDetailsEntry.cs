﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using SiraUtil.Web;
using SongDetailsCache;

namespace MorePlaylists.Entries
{
    internal abstract class SongDetailsEntry : GenericEntry
    {
        private List<Song> songs;

        public override async Task<List<Song>> GetSongs(IHttpService siraHttpService)
        {
            if (songs == null)
            {
                songs = new List<Song>();

                if (DownloadState == DownloadState.None)
                {
                    await DownloadPlaylist(siraHttpService);
                }

                if (DownloadState == DownloadState.Downloaded && RemotePlaylist is LegacyPlaylist playlist)
                {
                    SongDetails songDetails = await SongDetails.Init();
                    List<IPlaylistSong> playlistSongs = playlist.Distinct(IPlaylistSongComparer<LegacyPlaylistSong>.Default).ToList();
                    for (int i = 0; i < playlistSongs.Count; i++)
                    {
                        if (songDetails.songs.FindByHash(playlistSongs[i].Hash, out SongDetailsCache.Structs.Song song))
                        {
                            songs.Add(new Song(song.songName, $"{song.songAuthorName} [{song.levelAuthorName}]", song.coverURL));
                        }
                    }
                }
            }
            return songs;
        }
    }
}