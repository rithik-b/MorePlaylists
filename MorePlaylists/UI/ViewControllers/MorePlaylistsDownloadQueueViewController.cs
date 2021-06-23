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
using System.Diagnostics;
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
        internal static event Action<bool> QueueFull;
        internal CancellationTokenSource tokenSource = new CancellationTokenSource();

        public static readonly int MAX_SIMULTANEOUS_DOWNLOADS = 3;

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
                QueueFull?.Invoke(true);
            }
            customListTableData?.tableView?.ReloadData();
            UpdateDownloadingState(queuedPlaylist);
        }

        internal void UpdateDownloadingState(DownloadQueueItem item)
        {
            for (int i = 0; i < queueItems.Count; i++)
            {
                if ((queueItems[i] as DownloadQueueItem).playlistEntry.DownloadState == DownloadState.Downloaded || (queueItems[i] as DownloadQueueItem).playlistEntry.DownloadState == DownloadState.Error)
                {
                    //queueItems.Remove(i);
                    customListTableData?.tableView?.ReloadData();
                }
            }
            if (queueItems.Count == 0)
            {
                SongCore.Loader.Instance.RefreshSongs(false);
            }
            if (queueItems.Count < 3)
            {
                QueueFull?.Invoke(false);
            }
        }

        internal void DownloadAborted(DownloadQueueItem download)
        {
            if (queueItems.Contains(download))
            {
                queueItems.Remove(download);
            }
            if (queueItems.Count == 0)
            {
                SongCore.Loader.Instance.RefreshSongs(false);
            }
            customListTableData?.tableView?.ReloadData();
            if (queueItems.Count < 3)
            {
                QueueFull?.Invoke(false);
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

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
                        Task.Run(DownloadSongs);
                    }
                    else
                    {
                        if (downloadProgress is IProgress<float> progress)
                        {
                            progress.Report(1);
                            playlistEntry.DownloadState = DownloadState.Downloaded;
                        }
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

        private async Task DownloadSongs()
        {
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
            playlistEntry.DownloadState = DownloadState.Downloaded;
            MorePlaylistsDownloadQueueViewController.DidFinishDownloadingItem?.Invoke(this);
        }

        public void ProgressUpdate(float progressFloat)
        {
            Color color = SongCore.Utilities.HSBColor.ToColor(new SongCore.Utilities.HSBColor(Mathf.PingPong(progressFloat * 0.35f, 1), 1, 1));
            color.a = 0.35f;
            bgImage.color = color;
            bgImage.fillAmount = progressFloat;
        }

    }
}
