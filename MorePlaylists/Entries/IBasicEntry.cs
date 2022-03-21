using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using SiraUtil.Web;

namespace MorePlaylists.Entries;

public interface IBasicEntry : IEntry
{
    IPlaylist? RemotePlaylist { get; }
    Task CachePlaylist(IHttpService siraHttpService, CancellationToken cancellationToken = default);
}
