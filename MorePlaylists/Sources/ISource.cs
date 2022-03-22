using System;
using HMUI;
using MorePlaylists.UI;
using UnityEngine;

namespace MorePlaylists.Sources
{
    internal interface ISource
    {
        Sprite Logo { get; }
        IDetailViewController DetailViewController { get; }
        IListViewController ListViewController { get; }
        event Action<ViewController, ViewController.AnimationDirection>? ViewControllerRequested;
        event Action<ViewController, ViewController.AnimationDirection, Action?>? ViewControllerDismissRequested;
    }
}
