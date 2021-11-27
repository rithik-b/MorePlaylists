using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using Newtonsoft.Json;
using SiraUtil;
using UnityEngine;

namespace MorePlaylists.Sources
{
    internal class BeatSaver : ISource
    {
        private SiraClient siraClient;
        private Sprite _logo;

        public string Website => "https://api.beatsaver.com/";

        public string Endpoint => "playlists/search/0";

        public Sprite Logo
        {
            get
            {
                if (_logo == null)
                {
                    _logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.BeatSaver.png");
                }
                return _logo;
            }
        }

        public BeatSaver(SiraClient siraClient)
        {
            this.siraClient = siraClient;
        }

        public async Task<List<GenericEntry>> GetEndpointResultTask(bool refreshRequested, CancellationToken token, string searchQuery)
        {
            try
            {
                WebResponse webResponse = await siraClient.GetAsync(Website + Endpoint, token);
                if (webResponse.IsSuccessStatusCode)
                {
                    byte[] byteResponse = webResponse.ContentToBytes();
                    return JsonConvert.DeserializeObject<BeatSaverResponse>(Encoding.UTF8.GetString(byteResponse)).Entries.Cast<GenericEntry>().ToList();
                }
                else
                {
                    Plugin.Log.Info($"An error occurred while trying to fetch the {nameof(BeatSaver)} playlists\nError code: {webResponse.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Info($"An error occurred while trying to fetch the {nameof(BeatSaver)} playlists\nException: {e}");
            }
            return new List<GenericEntry>();
        }
    }
}
