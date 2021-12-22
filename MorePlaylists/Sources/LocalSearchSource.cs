using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using UnityEngine;

namespace MorePlaylists.Sources
{
    internal abstract class LocalSearchSource : ISource
    {
        public abstract Sprite Logo { get; }
        public bool PagingSupport => false;
        public abstract Task<List<GenericEntry>> GetEndpointResult(bool refreshRequested, bool resetPage, IProgress<float> progress, CancellationToken token, string searchQuery);

        protected List<GenericEntry> Search(List<GenericEntry> playlistEntries, string searchQuery)
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
