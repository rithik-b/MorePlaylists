using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;
using SiraUtil;

namespace MorePlaylists.Sources
{
    internal class BeastSaber : ISource, IInitializable
    {
        private SiraClient siraClient;
        private List<BeastSaberEntry> _endpointResult = new List<BeastSaberEntry>();
        private Sprite _logo;

        public string Website => "https://bsaber.com/";
        public string Endpoint => "PlaylistAPI/playlistAPI.json";
        public Sprite Logo => _logo;

        public BeastSaber(SiraClient siraClient)
        {
            this.siraClient = siraClient;
        }

        public void Initialize()
        {
            _logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.BeastSaber.png");
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
                        _endpointResult = JsonConvert.DeserializeObject<List<BeastSaberEntry>>(Encoding.UTF8.GetString(byteResponse));
                    }
                    else
                    {
                        Plugin.Log.Info($"An error occurred while trying to fetch the BeastSaber playlists\nError code: {webResponse.StatusCode}");
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Info($"An error occurred while trying to fetch the BeastSaber playlists\nException: {e}");
                }
            }
            return _endpointResult?.Cast<GenericEntry>().ToList();
        }
    }
}
