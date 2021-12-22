using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using SiraUtil.Web;
using UnityEngine;

namespace MorePlaylists.Sources
{
    internal class AccSaber : LocalSearchSource
    {
        private List<AccSaberEntry> cachedResult = new List<AccSaberEntry>();
        private readonly IHttpService siraHttpService;
        private Sprite _logo;

        public const string website = "https://api.accsaber.com/playlists/";
        public readonly string[] playlists = new string[] { "true", "standard", "tech", "overall" };

        public override Sprite Logo
        {
            get
            {
                if (_logo == null)
                {
                    _logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.AccSaber.png");
                }
                return _logo;
            }
        }

        public AccSaber(IHttpService siraHttpService)
        {
            this.siraHttpService = siraHttpService;
        }

        public override async Task<List<GenericEntry>> GetEndpointResult(bool refreshRequested, bool resetPage, IProgress<float> progress, CancellationToken token, string searchQuery)
        {
            if (cachedResult.Count == 0 || refreshRequested)
            {
                for (int i = 0; i < playlists.Length; i++)
                {
                    AccSaberEntry accSaberEntry = await AccSaberEntry.GetAccSaberPlaylist(website + playlists[i], siraHttpService);
                    if (accSaberEntry != null)
                    {
                        cachedResult.Add(accSaberEntry);
                    }
                    progress.Report(((float)i + 1) / playlists.Length);
                }
            }
            return Search(cachedResult.Cast<GenericEntry>().ToList(), searchQuery);
        }
    }
}
