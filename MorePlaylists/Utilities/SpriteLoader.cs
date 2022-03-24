﻿using BeatSaberPlaylistsLib.Types;
using MorePlaylists.Entries;
using SiraUtil;
using SiraUtil.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib;
using MorePlaylists.Sources;
using UnityEngine;

namespace MorePlaylists.Utilities
{
    internal class SpriteLoader : ICachable
    {
        private readonly IHttpService siraHttpService;
        private readonly ConcurrentDictionary<string, Sprite> cachedSprites;
        private readonly Queue<Action> spriteQueue;
        private readonly object queueLock;
        private bool coroutineRunning;

        public SpriteLoader(IHttpService siraHttpService)
        {
            this.siraHttpService = siraHttpService;
            cachedSprites = new ConcurrentDictionary<string, Sprite>();
            spriteQueue = new Queue<Action>();
            queueLock = new object();
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
                    cachedSprites.TryAdd(key, sprite);
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
            if (!coroutineRunning)
            {
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
            }
        }

        private static readonly YieldInstruction LoadWait = new WaitForEndOfFrame();
        private IEnumerator<YieldInstruction> SpriteLoadCoroutine()
        {
            lock (queueLock)
            {
                if (coroutineRunning)
                    yield break;
                coroutineRunning = true;
            }
            while (spriteQueue.Count > 0)
            {
                yield return LoadWait;
                if (spriteQueue.Count == 0)
                {
                    break;
                }
                var loader = spriteQueue.Dequeue();
                _ = IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => loader?.Invoke());
            }
            coroutineRunning = false;
            if (spriteQueue.Count > 0)
            {
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
            }
        }

        public void ClearCache()
        {
            cachedSprites.Clear();
            lock (queueLock)
            {
                spriteQueue.Clear();
            }
        }
    }
}
