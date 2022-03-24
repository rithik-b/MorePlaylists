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

namespace MorePlaylists.BeatSaver
{
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

        public BeatSaverFilterModel CurrentFilters => filtersViewController.filterOptions;
        public IListViewController ListViewController => listViewController;
        public IDetailViewController DetailViewController { get; }
        
        public event Action<ViewController, ViewController.AnimationDirection>? ViewControllerRequested;
        public event Action<ViewController, ViewController.AnimationDirection>? ViewControllerDismissRequested;

        public BeatSaver(UBinder<Plugin, PluginMetadata> metadata, BeatSaverFiltersViewController filtersViewController, BeatSaverListViewController listViewController, BeatSaverDetailViewController detailViewController)
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

        public async Task<List<IBeatSaverEntry>?> GetPage(bool refreshRequested, CancellationToken token)
        {
            List<IBeatSaverEntry>? entries = null;
            
            if (refreshRequested || page == null)
            {
                switch (CurrentFilters.FilterMode)
                {
                    case FilterMode.Search:
                        page = await beatSaverInstance.SearchPlaylists(CurrentFilters.NullableSearchFilter, token: token);
                        break;
                    
                    case FilterMode.User:
                        page = null;
                        if (CurrentFilters.UserName != null)
                        {
                            var user = await beatSaverInstance.User(CurrentFilters.UserName, token);
                            if (user != null)
                            {
                                page = await beatSaverInstance.UserPlaylists(user.ID, token: token);
                                entries = new List<IBeatSaverEntry> {new BeatSaverUserPlaylistEntry(user)};
                            }
                        }
                        break;
                }
                ExhaustedPlaylists = false;
            }
            else
            {
                var newPage = await page.Next(token);
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                page = newPage;
            }

            if (entries == null && (page == null || page.Empty))
            {
                ExhaustedPlaylists = true;
                return null;
            }

            entries ??= new List<IBeatSaverEntry>();
            if (page != null)
            {
                foreach (var playlist in page.Playlists)
                {
                    entries.Add(new BeatSaverPlaylistEntry(playlist));
                }   
            }
            return entries;
        }

        private void RequestFilterView() =>
            ViewControllerRequested?.Invoke(filtersViewController, ViewController.AnimationDirection.Vertical);

        private void ClearFilters() => filtersViewController.ClearFilters();

        private void OnFiltersSet(BeatSaverFilterModel filterOptions) => listViewController.SetActiveFilter(filterOptions);

        private void RequestFilterViewDismiss() => ViewControllerDismissRequested?.Invoke(filtersViewController, ViewController.AnimationDirection.Vertical);
    }
}
