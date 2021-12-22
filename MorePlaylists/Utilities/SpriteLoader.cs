using BeatSaberPlaylistsLib.Types;
using MorePlaylists.Entries;
using SiraUtil;
using SiraUtil.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MorePlaylists.Utilities
{
    internal class SpriteLoader
    {
        private readonly IHttpService siraHttpService;
        private readonly Dictionary<string, Sprite> cachedURLSprites;
        private readonly Dictionary<string, Sprite> cachedBase64Sprites;

        private readonly ConcurrentQueue<Action> spriteQueue;

        public SpriteLoader(IHttpService siraHttpService)
        {
            this.siraHttpService = siraHttpService;
            cachedURLSprites = new Dictionary<string, Sprite>();
            cachedBase64Sprites = new Dictionary<string, Sprite>();

            spriteQueue = new ConcurrentQueue<Action>();
        }

        public void GetSpriteForEntry(IGenericEntry entry, Action<Sprite> onCompletion)
        {
            switch (entry.SpriteType)
            {
                case SpriteType.URL:
                    DownloadSpriteAsync(entry.SpriteString, onCompletion);
                    break;
                case SpriteType.Base64:
                    ParseBase64Sprite(entry.SpriteString, onCompletion);
                    break;
                case SpriteType.Playlist:
                    GetPlaylistSprite(entry.RemotePlaylist as IDeferredSpriteLoad, onCompletion);
                    break;
            }
        }

        public async void DownloadSpriteAsync(string spriteURL, Action<Sprite> onCompletion)
        {
            // Check Cache
            if (cachedURLSprites.TryGetValue(spriteURL, out Sprite cachedSprite))
            {
                onCompletion?.Invoke(cachedSprite);
                return;
            }

            try
            {
                IHttpResponse webResponse = await siraHttpService.GetAsync(spriteURL, cancellationToken: CancellationToken.None).ConfigureAwait(false);
                byte[] imageBytes = await webResponse.ReadAsByteArrayAsync();
                QueueLoadSprite(spriteURL, cachedURLSprites, imageBytes, onCompletion);
            }
            catch (Exception)
            {
                onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
            }
        }

        public void ParseBase64Sprite(string base64, Action<Sprite> onCompletion)
        {
            // Check Cache
            if (cachedBase64Sprites.TryGetValue(base64, out Sprite cachedSprite))
            {
                onCompletion?.Invoke(cachedSprite);
                return;
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Utils.Base64ToByteArray(base64);
            }
            catch (FormatException)
            {
                imageBytes = Array.Empty<byte>();
            }

            QueueLoadSprite(base64, cachedBase64Sprites, imageBytes, onCompletion);
        }

        public void GetPlaylistSprite(IDeferredSpriteLoad playlist, Action<Sprite> onCompletion)
        {
            if (playlist.SpriteWasLoaded)
            {
                onCompletion?.Invoke(playlist.Sprite);
            }
            else
            {
                playlist.SpriteLoaded += (sender, args) =>
                {
                    onCompletion?.Invoke(playlist.Sprite);
                };
                _ = playlist.Sprite;
            }
        }

        private void QueueLoadSprite(string key, Dictionary<string, Sprite> cache, byte[] imageBytes, Action<Sprite> onCompletion)
        {
            spriteQueue.Enqueue(() =>
            {
                try
                {
                    Sprite sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                    sprite.texture.wrapMode = TextureWrapMode.Clamp;
                    cache[key] = sprite;
                    onCompletion?.Invoke(sprite);
                }
                catch (Exception)
                {
                    onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                }
            });
            SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
        }

        public static YieldInstruction LoadWait = new WaitForEndOfFrame();

        private IEnumerator<YieldInstruction> SpriteLoadCoroutine()
        {
            while (spriteQueue.TryDequeue(out var loader))
            {
                yield return LoadWait;
                loader?.Invoke();
            }
        }
    }
}
