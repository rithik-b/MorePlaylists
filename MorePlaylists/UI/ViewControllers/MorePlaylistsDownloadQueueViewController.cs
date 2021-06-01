using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Types;
using MorePlaylists.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    public class MorePlaylistsDownloadQueueViewController : BSMLResourceViewController, IInitializable, IDisposable
    {
        public override string ResourceName => "MorePlaylists.UI.Views.MorePlaylistsDownloadQueueView.bsml";
        internal static Action<DownloadQueueItem> didAbortDownload;
        internal static Action<DownloadQueueItem> didFinishDownloadingItem;
        internal CancellationTokenSource tokenSource = new CancellationTokenSource();

        [UIValue("download-queue")]
        internal List<object> queueItems = new List<object>();
        [UIComponent("download-list")]
        private CustomCellListTableData customListTableData;

        [UIAction("#post-parse")]
        internal void Setup() 
        { 
            customListTableData?.tableView?.ReloadData();
        }

        public void Initialize()
        {
            didAbortDownload += DownloadAborted;
            didFinishDownloadingItem += UpdateDownloadingState;
        }

        public void Dispose()
        {
            didAbortDownload -= DownloadAborted;
            didFinishDownloadingItem -= UpdateDownloadingState;
        }

        internal void EnqueuePlaylist(IGenericEntry playlistToDownload)
        {
            DownloadQueueItem queuedPlaylist = new DownloadQueueItem(playlistToDownload);
            queueItems.Add(queuedPlaylist);
            customListTableData?.tableView?.ReloadData();
            UpdateDownloadingState(queuedPlaylist);
        }

        internal void AbortAllDownloads()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            foreach (DownloadQueueItem item in queueItems.ToArray())
            {
                item.AbortDownload();
            }
        }

        internal void UpdateDownloadingState(DownloadQueueItem item)
        {
            foreach (DownloadQueueItem inQueue in queueItems.Where(x => (x as DownloadQueueItem).queueState == DownloadQueueItem.PlaylistQueueState.Queued).ToArray())
            {
                inQueue.DownloadPlaylistAsync();
            }
            foreach (DownloadQueueItem downloaded in queueItems.Where(x => (x as DownloadQueueItem).queueState == DownloadQueueItem.PlaylistQueueState.Downloaded).ToArray())
            {
                queueItems.Remove(downloaded);
                customListTableData?.tableView?.ReloadData();
            }
            if (queueItems.Count == 0)
            {
                SongCore.Loader.Instance.RefreshSongs(false);
            }
        }

        internal void DownloadAborted(DownloadQueueItem download)
        {
            if (queueItems.Contains(download))
                queueItems.Remove(download);

            if (queueItems.Count == 0)
                SongCore.Loader.Instance.RefreshSongs(false);
            customListTableData?.tableView?.ReloadData();
        }
    }

    internal class DownloadQueueItem : INotifyPropertyChanged
    {
        public IGenericEntry playlistToDownload;
        public PlaylistQueueState queueState = PlaylistQueueState.Queued;
        private ImageView bgImage;
        public Progress<double> downloadProgress;
        private float _downloadingProgess;
        public CancellationTokenSource tokenSource = new CancellationTokenSource();
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("playlist-cover")]
        private readonly ImageView playlistCoverView;

        [UIValue("playlist-name")]
        public string PlaylistName => playlistToDownload == null || playlistToDownload.Title == null ? " " : playlistToDownload.Title;

        [UIValue("playlist-author")]
        public string PlaylistAuthor => playlistToDownload == null || playlistToDownload.Author == null ? " " : playlistToDownload.Author;

        [UIAction("abort-clicked")]
        public void AbortDownload()
        {
            tokenSource.Cancel();
            MorePlaylistsDownloadQueueViewController.didAbortDownload?.Invoke(this);
        }

        public DownloadQueueItem()
        {
        }

        public DownloadQueueItem(IGenericEntry playlistToDownload)
        {
            this.playlistToDownload = playlistToDownload;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistAuthor)));
        }

        [UIAction("#post-parse")]
        public void Setup()
        {
            if (playlistCoverView == null)
            {
                return;
            }

            var filter = playlistCoverView.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            filter.aspectRatio = 1f;
            filter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth;
            playlistCoverView.sprite = playlistToDownload.Sprite;
            playlistCoverView.rectTransform.sizeDelta = new Vector2(8, 0);
            downloadProgress = new Progress<double>(ProgressUpdate);

            bgImage = playlistCoverView.transform.parent.gameObject.AddComponent<HMUI.ImageView>();
            bgImage.enabled = true;
            bgImage.sprite = Sprite.Create((new Texture2D(1, 1)), new Rect(0, 0, 1, 1), Vector2.one / 2f);
            bgImage.type = UnityEngine.UI.Image.Type.Filled;
            bgImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            bgImage.fillAmount = 0;
            bgImage.material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat;
        }

        public void ProgressUpdate(double progress)
        {
            _downloadingProgess = (float)progress;
            Color color = SongCore.Utilities.HSBColor.ToColor(new SongCore.Utilities.HSBColor(Mathf.PingPong(_downloadingProgess * 0.35f, 1), 1, 1));
            color.a = 0.35f;
            bgImage.color = color;
            bgImage.fillAmount = _downloadingProgess;
        }

        public async void DownloadPlaylistAsync()
        {
            queueState = PlaylistQueueState.Downloading;
            try
            {
                Stream playlistStream = new MemoryStream(await DownloaderUtils.instance.DownloadFileToBytesAsync(playlistToDownload.PlaylistURL, tokenSource.Token));
                BeatSaberPlaylistsLib.Types.IPlaylist playlist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(playlistStream);
                PlaylistLibUtils.SavePlaylist(playlist);
                queueState = PlaylistQueueState.Downloaded;
                MorePlaylistsDownloadQueueViewController.didFinishDownloadingItem?.Invoke(this);
            }
            catch (Exception e)
            {
                queueState = PlaylistQueueState.Error;
                MorePlaylistsDownloadQueueViewController.didFinishDownloadingItem?.Invoke(this);
            }

        }

        public enum PlaylistQueueState { Queued, Downloading, Downloaded, Error };
    }
}
