using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace MorePlaylists.Sources
{
    internal class BeastSaber : ISource, IInitializable
    {
        private List<BeastSaberEntry> _endpointResult = new List<BeastSaberEntry>();
        private Sprite _logo;

        public string Website => "https://bsaber.com/";
        public string Endpoint => "PlaylistAPI/playlistAPI.json";
        public Sprite Logo => _logo;

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
                    byte[] response = await DownloaderUtils.instance.DownloadFileToBytesAsync(Website + Endpoint, token);
                    _endpointResult = JsonConvert.DeserializeObject<List<BeastSaberEntry>>(Encoding.UTF8.GetString(response));
                }
                catch (Exception e)
                {
                }
            }
            return _endpointResult?.Cast<GenericEntry>().ToList();
        }
    }
}
