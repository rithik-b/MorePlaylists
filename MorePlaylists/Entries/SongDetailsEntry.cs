using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using SiraUtil.Web;
using SongDetailsCache;

namespace MorePlaylists.Entries
{
    internal abstract class SongDetailsEntry : BasicEntry
    {
        private List<Song>? songs;
        public override async Task<List<Song>?> GetSongs(IHttpService siraHttpService, CancellationToken cancellationToken, bool firstPage = false)
        {
            if (songs == null)
            {
                if (RemotePlaylist == null)
                {
                    await CachePlaylist(siraHttpService, cancellationToken);
                }

                if (RemotePlaylist != null && !cancellationToken.IsCancellationRequested)
                {
                    songs = new List<Song>();
                    var songDetails = await SongDetails.Init();
                    var playlistSongs = RemotePlaylist.Distinct(IPlaylistSongComparer<LegacyPlaylistSong>.Default).ToList();
                    for (var i = 0; i < playlistSongs.Count; i++)
                    {
                        if (songDetails.songs.FindByHash(playlistSongs[i].Hash, out var song))
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
