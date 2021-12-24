using MorePlaylists.Entries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePlaylists.Sources
{
    public interface ISource
    {
        Sprite Logo { get; }
        bool PagingSupport { get; }
        Task<List<GenericEntry>> GetEndpointResult(bool refreshRequested, bool resetPage, IProgress<float> progress, CancellationToken token, string searchQuery);
    }
}
