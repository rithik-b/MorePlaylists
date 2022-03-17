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
        public event Action<IBasicEntry>? FinishedCaching;
        
        #region Basic Entry
        
        private bool isCaching;
        public IPlaylist? RemotePlaylist { get; private set; }
        
        public async Task CachePlaylist(IHttpService siraHttpService, CancellationToken cancellationToken)
        {
            if (isCaching || RemotePlaylist != null)
            {
                return;
            }
            
            isCaching = true;
            try
            {
                var webResponse = await siraHttpService.GetAsync(PlaylistURL, cancellationToken: cancellationToken);
                if (webResponse.Successful)
                {
                    RemotePlaylist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(await webResponse.ReadAsStreamAsync());
                }
                else if (!cancellationToken.IsCancellationRequested)
                {
                    Plugin.Log?.Error("An error occurred while acquiring " + PlaylistURL + $"\nError code: {webResponse.Code}");
                }
            }
            catch (Exception e)
            {
                if (e is not TaskCanceledException)
                {
                    Plugin.Log?.Error("An exception occurred while acquiring " + PlaylistURL + $"\nException: {e.Message}");
                }
            }
            isCaching = false;
            FinishedCaching?.Invoke(this);
        }
        
        #endregion

        #region Abstract

        public abstract string Title { get; protected set; }
        public abstract string Author { get; protected set; }
        public abstract string Description { get; protected set; }
        public abstract string PlaylistURL { get; protected set; }
        public abstract string SpriteURL { get; protected set; }
        public abstract Task<List<Song>?> GetSongs(IHttpService siraHttpService, CancellationToken cancellationToken);

        #endregion
    }
}
