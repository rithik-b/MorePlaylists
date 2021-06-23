using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using MorePlaylists.Entries;
using MorePlaylists.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    public class MorePlaylistsDownloadQueueViewController : BSMLResourceViewController, IInitializable, IDisposable
    {
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsDownloadQueueView.bsml";
        internal static Action<DownloadQueueItem> DidAbortDownload;
        internal static Action<DownloadQueueItem> DidFinishDownloadingItem;

        [UIValue("download-queue")]
        internal List<object> queueItems = new List<object>();
        [UIComponent("download-list")]
        private CustomCellListTableData customListTableData;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            customListTableData?.tableView?.ReloadData();
        }

        public void Initialize()
        {
            DidAbortDownload += DownloadAborted;
            DidFinishDownloadingItem += UpdateDownloadingState;
        }

        public void Dispose()
        {
            DidAbortDownload -= DownloadAborted;
            DidFinishDownloadingItem -= UpdateDownloadingState;
        }

        internal void EnqueuePlaylist(IGenericEntry playlistToDownload, bool downloadSongs)
        {
            DownloadQueueItem queuedPlaylist = new DownloadQueueItem(playlistToDownload, downloadSongs);
            queueItems.Add(queuedPlaylist);
            if (queueItems.Count == 3)
            {
            }
            customListTableData?.tableView?.ReloadData();
            UpdateDownloadingState(queuedPlaylist);
        }

        internal void UpdateDownloadingState(DownloadQueueItem item)
        {
            foreach (DownloadQueueItem downloaded in queueItems.Where(x => (x as DownloadQueueItem).playlistEntry.DownloadState == DownloadState.Downloaded || (x as DownloadQueueItem).playlistEntry.DownloadState == DownloadState.Error).ToArray())
            {
                customListTableData?.data?.Remove(downloaded);
                customListTableData?.tableView?.ReloadData();
            }
        }

        internal void DownloadAborted(DownloadQueueItem download)
        {
            if (queueItems.Contains(download))
            {
                queueItems.Remove(download);
            }
            customListTableData?.tableView?.ReloadData();
        }
    }

    internal class DownloadQueueItem
    {
        public IGenericEntry playlistEntry;
        private ImageView bgImage;
        public Progress<float> downloadProgress;
        public CancellationTokenSource tokenSource;
        private bool downloadSongs;
        private bool initialized;
        private static SemaphoreSlim downloadSongsSemaphore = new SemaphoreSlim(1, 1);

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView;

        [UIComponent("playlist-name")]
        private TextMeshProUGUI playlistNameText;

        [UIComponent("playlist-author")]
        private TextMeshProUGUI playlistAuthorText;

        [UIAction("abort-clicked")]
        public void AbortDownload()
        {
            tokenSource.Cancel();
            playlistEntry.Owned = false;
            PlaylistLibUtils.DeletePlaylistIfExists(playlistEntry.Playlist);
            MorePlaylistsDownloadQueueViewController.DidAbortDownload?.Invoke(this);
        }

        public DownloadQueueItem()
        {
        }

        public DownloadQueueItem(IGenericEntry playlistEntry, bool downloadSongs)
        {
            this.playlistEntry = playlistEntry;
            this.downloadSongs = downloadSongs;
            tokenSource = new CancellationTokenSource();
            playlistEntry.Owned = true;
            initialized = false;
        }

        [UIAction("#post-parse")]
        public void Setup()
        {
            if (!initialized)
            {
                if (playlistEntry.DownloadState == DownloadState.DownloadedPlaylist)
                {
                    PlaylistEntry_FinishedDownload();
                }
                else
                {
                    playlistEntry.FinishedDownload += PlaylistEntry_FinishedDownload;
                }
            }
            initialized = true;

            if (playlistCoverView == null || playlistNameText == null || playlistAuthorText == null)
            {
                return;
            }

            var filter = playlistCoverView.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            filter.aspectRatio = 1f;
            filter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth;
            playlistCoverView.sprite = playlistEntry.Sprite;
            playlistCoverView.rectTransform.sizeDelta = new Vector2(8, 0);
            playlistNameText.text = playlistEntry.Title;
            playlistAuthorText.text = playlistEntry.Author;
            downloadProgress = new Progress<float>(ProgressUpdate);

            bgImage = playlistCoverView.transform.parent.gameObject.AddComponent<HMUI.ImageView>();
            bgImage.enabled = true;
            bgImage.sprite = Sprite.Create((new Texture2D(1, 1)), new Rect(0, 0, 1, 1), Vector2.one / 2f);
            bgImage.type = UnityEngine.UI.Image.Type.Filled;
            bgImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            bgImage.fillAmount = 0;
            bgImage.material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat;
        }

        private void PlaylistEntry_FinishedDownload()
        {
            playlistEntry.FinishedDownload -= PlaylistEntry_FinishedDownload;
            if (playlistEntry.DownloadState == DownloadState.DownloadedPlaylist)
            {
                try
                {
                    playlistEntry.Playlist.SetCustomData("syncURL", playlistEntry.PlaylistURL);
                    PlaylistLibUtils.SavePlaylist(playlistEntry.Playlist);

                    if (downloadSongs)
                    {
                        DownloadSongs();
                    }
                    else
                    {
                        if (downloadProgress is IProgress<float> progress)
                        {
                            progress.Report(1);
                        }
                        playlistEntry.DownloadState = DownloadState.Downloaded;
                        Plugin.Log.Debug(playlistEntry.DownloadState.ToString());
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Critical("An exception occurred while downloading. Exception: " + e.Message);
                    playlistEntry.DownloadState = DownloadState.Error;
                    playlistEntry.Owned = false;
                }
            }
            else
            {
                playlistEntry.Owned = false;
            }
            MorePlaylistsDownloadQueueViewController.DidFinishDownloadingItem?.Invoke(this);
        }

        private async void DownloadSongs()
        {
            // Song downloaded guarded by semaphore so songs cannot be duplicate downloaded
            await downloadSongsSemaphore.WaitAsync();

            List<IPlaylistSong> missingSongs = playlistEntry.Playlist.Where(s => s.PreviewBeatmapLevel == null).Distinct(IPlaylistSongComparer<LegacyPlaylistSong>.Default).ToList();
            for (int i = 0; i < missingSongs.Count; i++)
            {
                if (!string.IsNullOrEmpty(missingSongs[i].Hash))
                {
                    await DownloaderUtils.instance.BeatmapDownloadByHash(missingSongs[i].Hash, tokenSource.Token);
                }
                else if (!string.IsNullOrEmpty(missingSongs[i].Key))
                {
                    await DownloaderUtils.instance.BeatmapDownloadByKey(missingSongs[i].Key.ToLower(), tokenSource.Token);
                }

                // Update progress
                if (downloadProgress is IProgress<float> progress)
                {
                    progress.Report((float)(i + 1) / missingSongs.Count);
                }
            }
            SongCore.Loader.OnLevelPacksRefreshed += Loader_OnLevelPacksRefreshed;
            SongCore.Loader.Instance.RefreshSongs();

            downloadSongsSemaphore.Release();
            // If cancelled, restore to DownloadedPlaylist state
            if (tokenSource.IsCancellationRequested)
            {
                playlistEntry.DownloadState = DownloadState.DownloadedPlaylist;
            }
            else
            {
                playlistEntry.DownloadState = DownloadState.Downloaded;
            }
            MorePlaylistsDownloadQueueViewController.DidFinishDownloadingItem?.Invoke(this);
        }

        private static void Loader_OnLevelPacksRefreshed()
        {
            SongCore.Loader.OnLevelPacksRefreshed -= Loader_OnLevelPacksRefreshed;
            SongCore.Loader.Instance.RefreshSongs(false);
        }

        private void ProgressUpdate(float progressFloat)
        {
            Color color = SongCore.Utilities.HSBColor.ToColor(new SongCore.Utilities.HSBColor(Mathf.PingPong(progressFloat * 0.35f, 1), 1, 1));
            color.a = 0.35f;
            bgImage.color = color;
            bgImage.fillAmount = progressFloat;
        }

    }
}
