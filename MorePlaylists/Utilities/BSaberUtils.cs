using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Types;
using Newtonsoft.Json;
using UnityEngine;

namespace MorePlaylists.Utilities
{
    internal class BSaberUtils
    {
        public static readonly string WEBSITE_BASE_URL = "https://bsaber.com/";
        public static readonly string PLAYLIST_API_ENDPOINT = "PlaylistAPI/playlistAPI.json";
        public static readonly Sprite LOGO = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.BeastSaber.png");
        private static List<BSaberEntry> _endpointResult;

        public static async Task<List<BSaberEntry>> GetEndpointResultTask(bool refreshRequested, CancellationToken token)
        {
            if (_endpointResult == null || refreshRequested)
            {
                try
                {
                    byte[] response = await DownloaderUtils.instance.DownloadFileToBytesAsync(WEBSITE_BASE_URL + PLAYLIST_API_ENDPOINT, token);
                    _endpointResult = JsonConvert.DeserializeObject<List<BSaberEntry>>(Encoding.UTF8.GetString(response));
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
