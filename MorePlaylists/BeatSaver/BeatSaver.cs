using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp;
using BeatSaverSharp.Models.Pages;
using HMUI;
using IPA.Loader;
using MorePlaylists.Sources;
using MorePlaylists.UI;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;

namespace MorePlaylists.BeatSaver;

internal class BeatSaver : ISource, IInitializable, IDisposable
{
    private readonly BeatSaverSharp.BeatSaver beatSaverInstance;
    private readonly BeatSaverFiltersViewController filtersViewController;
    private readonly BeatSaverListViewController listViewController;
    
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

    public SearchTextPlaylistFilterOptions? CurrentFilters => filtersViewController.filterOptions;
    public IListViewController ListViewController => listViewController;
    public IDetailViewController DetailViewController { get; }
    
    public event Action<ViewController, ViewController.AnimationDirection>? ViewControllerRequested;
    public event Action<ViewController, ViewController.AnimationDirection, Action?>? ViewControllerDismissRequested;

    public BeatSaver(UBinder<Plugin, PluginMetadata> metadata, BeatSaverFiltersViewController filtersViewController, BeatSaverListViewController listViewController, BasicDetailViewController detailViewController)
    {
        var options = new BeatSaverOptions(metadata.Value.Name, metadata.Value.HVersion.ToString());
        beatSaverInstance = new BeatSaverSharp.BeatSaver(options);

        this.filtersViewController = filtersViewController;
        this.listViewController = listViewController;
        DetailViewController = detailViewController;
    }
    
    public void Initialize()
    {
        listViewController.FilterViewRequested += RequestFilterView;
        listViewController.FilterClearRequested += ClearFilters;
        
        filtersViewController.FiltersSet += OnFiltersSet;
        filtersViewController.RequestDismiss += RequestFilterViewDismiss;
    }

    public void Dispose()
    {
        listViewController.FilterViewRequested -= RequestFilterView;
        listViewController.FilterClearRequested -= ClearFilters;
        
        filtersViewController.FiltersSet -= OnFiltersSet;
        filtersViewController.RequestDismiss -= RequestFilterViewDismiss;
    }

    public async Task<List<BeatSaverEntry>?> GetPage(bool refreshRequested, CancellationToken token)
    {
        if (refreshRequested || page == null)
        {
            page = await beatSaverInstance.SearchPlaylists(CurrentFilters, token: token);
            ExhaustedPlaylists = false;
        }
        else
        {
            page = await page.Next(token);
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }

        if (page == null || page.Empty)
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

    private void RequestFilterView() =>
        ViewControllerRequested?.Invoke(filtersViewController, ViewController.AnimationDirection.Vertical);

    private void ClearFilters() => filtersViewController.ClearFilters();

    private void OnFiltersSet(SearchTextPlaylistFilterOptions? filterOptions)
    {
        listViewController.SetActiveFilter(filterOptions);
        RequestFilterViewDismiss();
    }

    private void RequestFilterViewDismiss() => ViewControllerDismissRequested?.Invoke(filtersViewController, ViewController.AnimationDirection.Vertical, null);
}
