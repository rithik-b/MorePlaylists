using MorePlaylists.UI;
using UnityEngine;

namespace MorePlaylists.Sources
{
    public interface ISource
    {
        Sprite Logo { get; }
        IDetailViewController DetailViewController { get; }
        IListViewController ListViewController { get; }
    }
}
