using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;

namespace MorePlaylists.Sources
{
    public interface IBasicSource : ISource
    {
        Task<List<IBasicEntry>?> GetEndpointResult(bool refreshRequested, IProgress<float> progress, CancellationToken token);
    }
}
