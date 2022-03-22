using BeatSaberPlaylistsLib.Types;
using MorePlaylists.Entries;
using SiraUtil;
using SiraUtil.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib;
using UnityEngine;

namespace MorePlaylists.Utilities
{
    internal class SpriteLoader
    {
        private readonly IHttpService siraHttpService;
        private readonly Dictionary<string, Sprite> cachedSprites;
        private readonly ConcurrentQueue<Action> spriteQueue;

        public SpriteLoader(IHttpService siraHttpService)
        {
            this.siraHttpService = siraHttpService;
            cachedSprites = new Dictionary<string, Sprite>();
            spriteQueue = new ConcurrentQueue<Action>();
        }
        
        public async Task DownloadSpriteAsync(string spriteURL, Action<Sprite> onCompletion, CancellationToken cancellationToken = default)
        {
            // Check Cache
            if (cachedSprites.TryGetValue(spriteURL, out var cachedSprite))
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    onCompletion?.Invoke(cachedSprite);
                }
                return;
            }

            try
            {
                var webResponse = await siraHttpService.GetAsync(spriteURL, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (webResponse.Successful)
                {
                    using var responseStream = await webResponse.ReadAsStreamAsync();
                    using var downscaledStream = await Task.Run(() => BeatSaberPlaylistsLib.Utilities.DownscaleImage(responseStream, 128), cancellationToken);
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var imageBytes = downscaledStream.ToArray();
                        QueueLoadSprite(spriteURL, imageBytes, onCompletion, cancellationToken);
                    }
                }
                else if (!cancellationToken.IsCancellationRequested)
                {
                    onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                }
            }
            catch (Exception)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                }
            }
        }

        private void QueueLoadSprite(string key, byte[] imageBytes, Action<Sprite> onCompletion, CancellationToken cancellationToken)
        {
            spriteQueue.Enqueue(() =>
            {
                try
                {
                    var sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                    sprite.texture.wrapMode = TextureWrapMode.Clamp;
                    cachedSprites[key] = sprite;
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        onCompletion?.Invoke(sprite);
                    }
                }
                catch (Exception)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                    }
                }
            });
            SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
        }
        
        private static readonly YieldInstruction LoadWait = new WaitForEndOfFrame();
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
