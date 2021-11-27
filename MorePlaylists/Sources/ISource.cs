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
        Task<List<GenericEntry>> GetEndpointResultTask(bool refreshRequested, CancellationToken token, string searchQuery);
    }
}
