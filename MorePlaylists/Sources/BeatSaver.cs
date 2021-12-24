using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using Newtonsoft.Json;
using SiraUtil.Web;
using UnityEngine;

namespace MorePlaylists.Sources
{
    internal class BeatSaver : ISource
    {
        private IHttpService _siraHttpService;
        private Sprite _logo;
        private int page;

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

        public bool PagingSupport => true;

        public BeatSaver(IHttpService siraHttpService)
        {
            _siraHttpService = siraHttpService;
        }

        public async Task<List<GenericEntry>> GetEndpointResult(bool refreshRequested, bool resetPage, IProgress<float> progress, CancellationToken token, string searchQuery)
        {
            if (resetPage)
            {
                page = 0;
            }

            try
            {
                IHttpResponse webResponse = await _siraHttpService.GetAsync($"https://api.beatsaver.com/playlists/search/{page}?q={searchQuery}", progress, token);
                if (webResponse.Successful)
                {
                    List<GenericEntry> returnVal = JsonConvert.DeserializeObject<BeatSaverResponse>(await webResponse.ReadAsStringAsync()).Entries.Cast<GenericEntry>().ToList();
                    page++;
                    return returnVal;
                }
                else
                {
                    Plugin.Log.Info($"An error occurred while trying to fetch the {nameof(BeatSaver)} playlists\nError code: {webResponse.Code}");
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
