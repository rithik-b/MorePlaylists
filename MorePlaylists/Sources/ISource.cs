using MorePlaylists.Entries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HMUI;
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
