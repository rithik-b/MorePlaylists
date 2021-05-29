using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Legacy;
using Newtonsoft.Json;

namespace MorePlaylists.Utilities
{
    internal class BSaberUtils
    {
        public static readonly string WEBSITE_BASE_URL = "https://bsaber.com/";
        public static readonly string PLAYLIST_API_ENDPOINT = "PlaylistAPI/playlistAPI.json";
        private static List<LegacyPlaylist> _endpointResult;

        public static async Task<List<LegacyPlaylist>> GetEndpointResultTask(bool refreshRequested, CancellationToken token)
        {
            if (_endpointResult == null || refreshRequested)
            {
                try
                {
                    byte[] response = await DownloaderUtils.instance.DownloadFileToBytesAsync(WEBSITE_BASE_URL + PLAYLIST_API_ENDPOINT, token);
                    _endpointResult = JsonConvert.DeserializeObject<List<LegacyPlaylist>>(Encoding.ASCII.GetString(response));
                }
                catch (Exception e)
                {
                    if (!(e is TaskCanceledException))
                    {
                    }
                }
            }
            return _endpointResult;
        }
    }
}
