using MorePlaylists.Entries;
using Newtonsoft.Json;
using SiraUtil;
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
        private SiraClient siraClient;
        private static List<HitbloqEntry> _endpointResult = new List<HitbloqEntry>();
        private Sprite _logo;

        public string Website => "https://hitbloq.com/";
        public string Endpoint => "api/map_pools_detailed";
        public Sprite Logo => _logo;

        public Hitbloq(SiraClient siraClient)
        {
            this.siraClient = siraClient;
        }

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
                    WebResponse webResponse = await siraClient.GetAsync(Website + Endpoint, token);
                    if (webResponse.IsSuccessStatusCode)
                    {
                        byte[] byteResponse = webResponse.ContentToBytes();
                        _endpointResult = JsonConvert.DeserializeObject<List<HitbloqEntry>>(Encoding.UTF8.GetString(byteResponse));
                    }
                    else
                    {
                        Plugin.Log.Info($"An error occurred while trying to fetch the HitBloq playlists\nError code: {webResponse.StatusCode}");
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Info($"An error occurred while trying to fetch the HitBloq playlists\nException: {e}");
                }
            }
            return _endpointResult.Cast<GenericEntry>().Reverse().ToList();
        }
    }
}
