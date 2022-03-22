using System;
using System.Threading;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.UI;
using MorePlaylists.Utilities;
using UnityEngine;
using Zenject;

namespace MorePlaylists.BeatSaver;

[HotReload(RelativePathToLayout = @".\BeatSaverDetailView.bsml")]
[ViewDefinition("MorePlaylists.BeatSaver.BeatSaverDetailView.bsml")]
internal class BeatSaverDetailViewController : BSMLAutomaticViewController, IDetailViewController
{
    [Inject]
    private readonly SpriteLoader spriteLoader = null!;

    [Inject] 
    private readonly MaterialGrabber materialGrabber = null!;

    [Inject] 
    private readonly BeatSaverFiltersViewController beatSaverFiltersViewController = null!;
    
    private BeatSaverEntry? selectedPlaylistEntry;
    private CancellationTokenSource? spriteLoadTokenSource;
    
    public ViewController ViewController => this;
    public event Action<IEntry, bool>? DidPressDownload;
    public event Action<IPlaylist>? DidGoToPlaylist;

    [UIComponent("playlist-cover")]
    private readonly ImageView playlistCoverView = null!;
    
    [UIComponent("user-image")]
    private readonly ImageView userImageView = null!;

    [UIComponent("text-page")]
    private readonly TextPageScrollView descriptionTextPage = null!;

    #region Actions

    [UIAction("#post-parse")]
    private void PostParse()
    {
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        playlistCoverView.material = materialGrabber.NoGlowRoundEdge;
        userImageView.material = materialGrabber.NoGlowRoundEdge;
    }

    [UIAction("author-click")]
    private void AuthorPressed()
    {
        beatSaverFiltersViewController.filterOptions.FilterMode = FilterMode.User;
        beatSaverFiltersViewController.filterOptions.UserName = PlaylistAuthor;
        beatSaverFiltersViewController.RaiseFiltersSet();
    }

    [UIAction("download-click")]
    private void DownloadPressed()
    {
        if (selectedPlaylistEntry != null)
        {
            DidPressDownload?.Invoke(selectedPlaylistEntry, false);
            NotifyPropertyChanged(nameof(DownloadInteractable));
        }
    }

    [UIAction("download-all-click")]
    private void DownloadAllPressed()
    {
        if (selectedPlaylistEntry != null)
        {
            DidPressDownload?.Invoke(selectedPlaylistEntry, true);
            NotifyPropertyChanged(nameof(DownloadInteractable));
        }
    }

    [UIAction("go-to-playlist")]
    private void GoToPlaylist()
    {
        if (selectedPlaylistEntry?.LocalPlaylist != null)
        {
            DidGoToPlaylist?.Invoke(selectedPlaylistEntry.LocalPlaylist);
        }
    }

    #endregion

    public void ShowDetail(IEntry selectedPlaylistEntry)
    {
        if (selectedPlaylistEntry is BeatSaverEntry beatSaverEntry)
        {
            this.selectedPlaylistEntry = beatSaverEntry;
            NotifyPropertyChanged(nameof(DownloadInteractable));
            NotifyPropertyChanged(nameof(DownloadActive));
            NotifyPropertyChanged(nameof(GoToActive));
            NotifyPropertyChanged(nameof(PlaylistName));
            NotifyPropertyChanged(nameof(PlaylistAuthor));
            NotifyPropertyChanged(nameof(PlaylistDescription));
            descriptionTextPage.ScrollTo(0, true);
            
            spriteLoadTokenSource?.Cancel();
            spriteLoadTokenSource?.Dispose();
            spriteLoadTokenSource = new CancellationTokenSource();
            _ = spriteLoader.DownloadSpriteAsync(beatSaverEntry.SpriteURL, sprite => playlistCoverView.sprite = sprite, spriteLoadTokenSource.Token);
            userImageView.sprite = BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;
            _ = spriteLoader.DownloadSpriteAsync(beatSaverEntry.Owner.Avatar, sprite => userImageView.sprite = sprite, spriteLoadTokenSource.Token);
        }
    }

    public void OnPlaylistDownloaded()
    {
        NotifyPropertyChanged(nameof(DownloadInteractable));
        NotifyPropertyChanged(nameof(DownloadActive));
        NotifyPropertyChanged(nameof(GoToActive));
    }

    #region Values

    [UIValue("playlist-name")] 
    public string PlaylistName
    {
        get
        {
            if (selectedPlaylistEntry != null)
            {
                if (selectedPlaylistEntry.Title.Length > 32)
                {
                    return selectedPlaylistEntry.Title.Substring(0, 28) + "...";
                }
                return selectedPlaylistEntry.Title;
            }
            return string.Empty;
        }
    }

    [UIValue("playlist-author")]
    public string PlaylistAuthor
    {
        get
        {
            if (selectedPlaylistEntry != null)
            {
                if (selectedPlaylistEntry.Author.Length > 32)
                {
                    return selectedPlaylistEntry.Author.Substring(0, 28) + "...";
                }
                return selectedPlaylistEntry.Author;
            }
            return string.Empty;
        }
    }

    [UIValue("playlist-description")]
    private string PlaylistDescription => string.IsNullOrWhiteSpace(selectedPlaylistEntry?.Description) ? "No Description available for this playlist." : selectedPlaylistEntry?.Description ?? "";

    [UIValue("download-interactable")]
    public bool DownloadInteractable => selectedPlaylistEntry is {DownloadBlocked: false};

    [UIValue("download-active")]
    public bool DownloadActive => selectedPlaylistEntry is {LocalPlaylist: null};

    [UIValue("go-to-active")]
    public bool GoToActive => selectedPlaylistEntry is {LocalPlaylist: { }};
    
    #endregion
}
