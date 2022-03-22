using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;

namespace MorePlaylists.Sources
{
    internal interface IBasicSource : ISource
    {
        Task<List<IBasicEntry>?> GetEndpointResult(bool refreshRequested, CancellationToken token);
    }
}
