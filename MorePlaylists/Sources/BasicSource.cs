using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using MorePlaylists.UI;
using Newtonsoft.Json;
using SiraUtil.Web;
using UnityEngine;

namespace MorePlaylists.Sources
{
    internal abstract class BasicSource<T> : IBasicSource where T : IBasicEntry
    {
        protected abstract string Website { get; }
        protected abstract string Endpoint { get; }
        public abstract Sprite Logo { get; }

        private List<T>? cachedResult;
        private IHttpService SiraHttpService { get; }
        public IListViewController ListViewController { get; }
        public IDetailViewController DetailViewController { get; }

        protected BasicSource(IHttpService siraHttpService, BasicListViewController listViewController, BasicDetailViewController detailViewController)
        {
            SiraHttpService = siraHttpService;
            ListViewController = listViewController;
            DetailViewController = detailViewController;
        }
        
        public async Task<List<IBasicEntry>?> GetEndpointResult(bool refreshRequested, CancellationToken token)
        {
            if (cachedResult == null || refreshRequested)
            {
                try
                {
                    var webResponse = await SiraHttpService.GetAsync(Website + Endpoint, cancellationToken: token);
                    if (webResponse.Successful)
                    {
                        cachedResult = JsonConvert.DeserializeObject<List<T>>(await webResponse.ReadAsStringAsync());
                    }
                    else
                    {
                        Plugin.Log?.Info($"An error occurred while trying to fetch the {typeof(T)} playlists\nError code: {webResponse.Code}");
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log?.Info($"An error occurred while trying to fetch the {typeof(T)} playlists\nException: {e}");
                }
            }
            return cachedResult?.Cast<IBasicEntry>().ToList();
        }
    }
}
