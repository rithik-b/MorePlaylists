using MorePlaylists.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePlaylists.Utilities
{
    internal class HitbloqUtils
    {
        public static readonly string WEBSITE_BASE_URL = "https://hitbloq.com/";
        public static readonly string PLAYLIST_API_ENDPOINT = "api/map_pools_detailed";
        public static readonly Sprite LOGO = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.Hitbloq.png");
        private static List<HitbloqEntry> _endpointResult;

        public static async Task<List<HitbloqEntry>> GetEndpointResultTask(bool refreshRequested, CancellationToken token)
        {
            if (_endpointResult == null || refreshRequested)
            {
                try
                {
                    byte[] response = await DownloaderUtils.instance.DownloadFileToBytesAsync(WEBSITE_BASE_URL + PLAYLIST_API_ENDPOINT, token);
                    _endpointResult = JsonConvert.DeserializeObject<List<HitbloqEntry>>(Encoding.UTF8.GetString(response));
                    foreach (HitbloqEntry hitbloqEntry in _endpointResult)
                    {
                        await hitbloqEntry.DownloadImage(token);
                    }
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
