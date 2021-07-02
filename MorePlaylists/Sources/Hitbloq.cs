using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MorePlaylists.Sources
{
    internal class Hitbloq : ISource, IInitializable
    {
        private static List<HitbloqEntry> _endpointResult = new List<HitbloqEntry>();
        private Sprite _logo;

        public string Website => "https://hitbloq.com/";
        public string Endpoint => "api/map_pools_detailed";
        public Sprite Logo => _logo;

        public void Initialize()
        {
            _logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.Hitbloq.png");
        }

        public async Task<List<GenericEntry>> GetEndpointResultTask(bool refreshRequested, CancellationToken token)
        {
            if (_endpointResult.Count == 0 || refreshRequested)
            {
                try
                {
                    byte[] response = await DownloaderUtils.instance.DownloadFileToBytesAsync(Website + Endpoint, token);
                    _endpointResult = JsonConvert.DeserializeObject<List<HitbloqEntry>>(Encoding.UTF8.GetString(response));
                    foreach (HitbloqEntry hitbloqEntry in _endpointResult)
                    {
                        await hitbloqEntry.DownloadImage(token);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Warn($"An error occurred while trying to fetch the HitBloq playlists\nException: {e}");
                }
            }
            return _endpointResult.Cast<GenericEntry>().ToList();
        }
    }
}
