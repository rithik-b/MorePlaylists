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
    internal abstract class LocalSearchSource<T> : ISource
    {
        private List<T> cachedResult = new List<T>();
        protected abstract IHttpService SiraHttpService { get; }
        public abstract string Website { get; }
        public abstract string Endpoint { get; }
        public abstract Sprite Logo { get; }
        public bool PagingSupport => false;

        public async Task<List<GenericEntry>> GetEndpointResult(bool refreshRequested, bool resetPage, CancellationToken token, string searchQuery)
        {
            if (cachedResult.Count == 0 || refreshRequested)
            {
                try
                {
                    IHttpResponse webResponse = await SiraHttpService.GetAsync(Website + Endpoint, cancellationToken: token);
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

        private List<GenericEntry> Search(List<GenericEntry> playlistEntries, string searchQuery)
        {
            if (searchQuery == null)
            {
                return playlistEntries;
            }

            return playlistEntries.Where(p => p.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    p.Author.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 || p.Description.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }
    }
}
