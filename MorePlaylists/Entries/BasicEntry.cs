using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;

namespace MorePlaylists.Entries
{
    public abstract class BasicEntry : IBasicEntry
    {
        public IPlaylist? LocalPlaylist { get; set; }
        public bool DownloadBlocked { get; set; }
        public bool ExhaustedPages => true;
        
        #region Basic Entry

        private readonly SemaphoreSlim cacheSemaphore = new(1, 1);
        public IPlaylist? RemotePlaylist { get; private set; }

        public async Task CachePlaylist(IHttpService siraHttpService, CancellationToken cancellationToken)
        {
            if (RemotePlaylist != null)
            {
                return;
            }

            await cacheSemaphore.WaitAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            try
            {
                var webResponse = await siraHttpService.GetAsync(PlaylistURL, cancellationToken: cancellationToken);
                if (webResponse.Successful)
                {
                    using var playlistStream = await webResponse.ReadAsStreamAsync();
                    RemotePlaylist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(playlistStream);
                }
                else if (!cancellationToken.IsCancellationRequested)
                {
                    Plugin.Log?.Error("An error occurred while acquiring " + PlaylistURL +
                                      $"\nError code: {webResponse.Code}");
                }
            }
            catch (Exception e)
            {
                if (e is not TaskCanceledException)
                {
                    Plugin.Log?.Error("An exception occurred while acquiring " + PlaylistURL +
                                      $"\nException: {e.Message}");
                }
            }
            finally
            {
                cacheSemaphore.Release();
            }
        }
        
        public async Task<IPlaylist?> DownloadPlaylist(IHttpService siraHttpService, CancellationToken cancellationToken = default)
        {
            if (RemotePlaylist != null)
            {
                return RemotePlaylist;
            }
            
            await cacheSemaphore.WaitAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            cacheSemaphore.Release();

            if (RemotePlaylist == null)
            {
                await CachePlaylist(siraHttpService, cancellationToken);
            }
            return RemotePlaylist;
        }
        
        #endregion

        #region Abstract

        public abstract string Title { get; protected set; }
        public abstract string Author { get; protected set; }
        public abstract string Description { get; protected set; }
        public abstract string PlaylistURL { get; protected set; }
        public abstract string SpriteURL { get; protected set; }
        public abstract Task<List<Song>?> GetSongs(IHttpService siraHttpService, CancellationToken cancellationToken, bool firstPage = false);

        #endregion
    }
}
