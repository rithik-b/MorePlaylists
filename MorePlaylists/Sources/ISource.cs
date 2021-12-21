using MorePlaylists.Entries;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePlaylists.Sources
{
    public interface ISource
    {
        string Website { get; }
        string Endpoint { get; }
        Sprite Logo { get; }
        bool PagingSupport { get; }
        Task<List<GenericEntry>> GetEndpointResult(bool refreshRequested, bool resetPage, CancellationToken token, string searchQuery);
    }
}
