using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using Newtonsoft.Json;
using SiraUtil.Web;

namespace MorePlaylists.Sources
{
    internal abstract class SinglePageSource<T> : LocalSearchSource
    {
        private List<T> cachedResult = new List<T>();
        protected abstract IHttpService SiraHttpService { get; }
        public abstract string Website { get; }
        public abstract string Endpoint { get; }

        public override async Task<List<GenericEntry>> GetEndpointResult(bool refreshRequested, bool resetPage, IProgress<float> progress, CancellationToken token, string searchQuery)
        {
            if (cachedResult.Count == 0 || refreshRequested)
            {
                try
                {
                    var webResponse = await SiraHttpService.GetAsync(Website + Endpoint, progress, token);
                    if (webResponse.Successful)
                    {
                        cachedResult = JsonConvert.DeserializeObject<List<T>>(await webResponse.ReadAsStringAsync());
                    }
                    else
                    {
                        Plugin.Log.Info($"An error occurred while trying to fetch the {typeof(T)} playlists\nError code: {webResponse.Code}");
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Info($"An error occurred while trying to fetch the {typeof(T)} playlists\nException: {e}");
                }
            }
            return Search(cachedResult.Cast<GenericEntry>().ToList(), searchQuery);
        }
    }
}
