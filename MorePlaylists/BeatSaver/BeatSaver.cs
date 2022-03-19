using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp;
using BeatSaverSharp.Models.Pages;
using IPA.Loader;
using MorePlaylists.Sources;
using MorePlaylists.UI;
using SiraUtil.Zenject;
using UnityEngine;

namespace MorePlaylists.BeatSaver;

internal class BeatSaver : ISource
{
    private readonly BeatSaverSharp.BeatSaver beatSaverInstance;
    private PlaylistPage? page;
    public bool ExhaustedPlaylists { get; private set; }

    private Sprite? logo;
    public Sprite Logo
    {
        get
        {
            if (logo == null)
            {
                logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.BeatSaver.png");
            }
            return logo;
        }
    }
    public IListViewController ListViewController { get; }
    public IDetailViewController DetailViewController { get; }
    
    public BeatSaver(UBinder<Plugin, PluginMetadata> metadata, BeatSaverListViewController listViewController, BasicDetailViewController detailViewController)
    {
        var options = new BeatSaverOptions(metadata.Value.Name, metadata.Value.HVersion.ToString());
        beatSaverInstance = new BeatSaverSharp.BeatSaver(options);
        
        ListViewController = listViewController;
        DetailViewController = detailViewController;
    }

    public async Task<List<BeatSaverEntry>?> GetPage(bool refreshRequested, CancellationToken token)
    {
        if (refreshRequested || page == null)
        {
            page = await beatSaverInstance.SearchPlaylists(token: token);
            ExhaustedPlaylists = false;
        }
        else
        {
            page = await page.Next(token);
        }

        if (page == null)
        {
            return null;
        }

        if (page.Empty)
        {
            ExhaustedPlaylists = true;
            return null;
        }

        var entries = new List<BeatSaverEntry>();
        foreach (var playlist in page.Playlists)
        {
            entries.Add(new BeatSaverEntry(playlist));
        }
        return entries;
    }
}
