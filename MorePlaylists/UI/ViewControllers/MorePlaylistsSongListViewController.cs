using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using SongDetailsCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    [HotReload(RelativePathToLayout = @"..\Views\MorePlaylistsSongListView.bsml")]
    [ViewDefinition("MorePlaylists.UI.Views.MorePlaylistsSongListView.bsml")]
    public class MorePlaylistsSongListViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private StandardLevelDetailViewController standardLevelDetailViewController;
        private IVRPlatformHelper platformHelper;

        private LoadingControl loadingSpinner;
        private HttpClient httpClient;
        private IGenericEntry playlistEntry;
        private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private static CancellationTokenSource tokenSource;
        private static SemaphoreSlim songLoadSemaphore = new SemaphoreSlim(1, 1);

        [UIComponent("list")]
        private readonly CustomListTableData customListTableData;

        [UIComponent("scroll-view")]
        private readonly ScrollView bsmlScrollView;

        [UIComponent("loading-modal")]
        public RectTransform loadingModal;

        [UIParams]
        internal BSMLParserParams parserParams;

        [Inject]
        public void Construct(StandardLevelDetailViewController standardLevelDetailViewController, IVRPlatformHelper platformHelper)
        {
            this.standardLevelDetailViewController = standardLevelDetailViewController;
            this.platformHelper = platformHelper;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation)
            {
                SetLoading(false);
                loadingModal.gameObject.SetActive(false);
                ClearList();
            }
        }

        public void Initialize()
        {
            httpClient = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false
            })
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            httpClient.DefaultRequestHeaders.Add("User-Agent", nameof(MorePlaylists));
            tokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            httpClient.CancelPendingRequests();
            httpClient.Dispose();
            tokenSource.Cancel();
            tokenSource.Dispose();
        }

        internal void SetCurrentPlaylist(IGenericEntry playlistEntry)
        {
            if (customListTableData == null)
            {
                return;
            }

            AbortLoading();
            SetLoading(true, 0);

            if (this.playlistEntry != null)
            {
                this.playlistEntry.FinishedDownload -= InitSongList;
            }
            this.playlistEntry = playlistEntry;

            ClearList();

            if (playlistEntry.DownloadState == DownloadState.Error)
            {
                SetLoading(false);
            }
            else if (playlistEntry.DownloadState == DownloadState.DownloadedPlaylist || playlistEntry.DownloadState == DownloadState.Downloaded)
            {
                InitSongList();
            }
            else
            {
                playlistEntry.FinishedDownload += InitSongList;
            }
        }

        internal void ClearList()
        {
            customListTableData.data.Clear();
            customListTableData.tableView.ReloadData();
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            loadingSpinner = GameObject.Instantiate(Accessors.LoadingControlAccessor(ref standardLevelDetailViewController), loadingModal);
            Destroy(loadingSpinner.GetComponent<Touchable>());

            ScrollView scrollView = customListTableData.tableView.GetComponent<ScrollView>();
            Accessors.PlatformHelperAccessor(ref scrollView) = platformHelper;
            Utils.TransferScrollBar(bsmlScrollView, scrollView);
        }

        [UIAction("list-select")]
        private void Select(TableView _, int __)
        {
            customListTableData.tableView.ClearSelection();
        }

        private async void InitSongList()
        {
            await songLoadSemaphore.WaitAsync();
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            ClearList();
            SetLoading(true, 0);

            if (customListTableData.data.Count == 0)
            {
                if (playlistEntry.RemotePlaylist is LegacyPlaylist playlist)
                {
                    SongDetails songDetails = await SongDetails.Init();
                    List<IPlaylistSong> playlistSongs = playlist.Distinct(IPlaylistSongComparer<LegacyPlaylistSong>.Default).ToList();
                    for (int i = 0; i < playlistSongs.Count; i++)
                    {
                        if (songDetails.songs.FindByHash(playlistSongs[i].Hash, out SongDetailsCache.Structs.Song song))
                        {
                            Sprite sprite = await LoadSpriteAsync(song);
                            customListTableData.data.Add(new CustomListTableData.CustomCellInfo(song.songName, $"{song.songAuthorName} [{song.levelAuthorName}]", sprite));
                        }
                        SetLoading(true, (float)i / (float)playlistSongs.Count);
                    }
                }
                customListTableData.tableView.ReloadData();
            }
            songLoadSemaphore.Release();
            SetLoading(false);
        }

        [UIAction("abort-click")]
        internal void AbortLoading()
        {
            SetLoading(false);
            tokenSource.Cancel();
        }

        private async Task<Sprite> LoadSpriteAsync(SongDetailsCache.Structs.Song song)
        {
            var path = song.coverURL;

            if (spriteCache.TryGetValue(path, out Sprite sprite))
                return sprite;

            try
            {
                using (var resp = await httpClient.GetAsync(path, HttpCompletionOption.ResponseContentRead, tokenSource.Token))
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        var imageBytes = await resp.Content.ReadAsByteArrayAsync();

                        if (spriteCache.TryGetValue(path, out sprite))
                            return sprite;

                        sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                        sprite.texture.wrapMode = TextureWrapMode.Clamp;
                        spriteCache[path] = sprite;

                        return sprite;
                    }
                }
            }
            catch { }

            return SongCore.Loader.defaultCoverImage;
        }

        private void SetLoading(bool value, double progress = 0, string details = "Loading Songs")
        {
            if (value && isActiveAndEnabled)
            {
                parserParams.EmitEvent("open-loading-modal");
                loadingSpinner.ShowDownloadingProgress(details, (float)progress);
            }
            else
            {
                parserParams.EmitEvent("close-loading-modal");
            }
        }
    }
}
