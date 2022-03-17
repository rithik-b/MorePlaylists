﻿using BeatSaberPlaylistsLib.Types;
using MorePlaylists.Entries;
using SiraUtil;
using SiraUtil.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        
        public void GetSpriteForEntry(IEntry entry, Action<Sprite> onCompletion) => _ = DownloadSpriteAsync(entry.SpriteURL, onCompletion);
        
        public async Task DownloadSpriteAsync(string spriteURL, Action<Sprite> onCompletion)
        {
            // Check Cache
            if (cachedSprites.TryGetValue(spriteURL, out var cachedSprite))
            {
                onCompletion?.Invoke(cachedSprite);
                return;
            }

            try
            {
                var webResponse = await siraHttpService.GetAsync(spriteURL, cancellationToken: CancellationToken.None).ConfigureAwait(false);
                var imageBytes = await webResponse.ReadAsByteArrayAsync();
                QueueLoadSprite(spriteURL, imageBytes, onCompletion);
            }
            catch (Exception)
            {
                onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
            }
        }

        private void QueueLoadSprite(string key, byte[] imageBytes, Action<Sprite> onCompletion)
        {
            spriteQueue.Enqueue(() =>
            {
                try
                {
                    var sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                    sprite.texture.wrapMode = TextureWrapMode.Clamp;
                    cachedSprites[key] = sprite;
                    onCompletion?.Invoke(sprite);
                }
                catch (Exception)
                {
                    onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
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
