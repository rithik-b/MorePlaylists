using System;
using System.Collections.Generic;
using System.Threading;
using SiraUtil.Web;
using UnityEngine;

namespace MorePlaylists.Utilities
{
    internal class SpriteLoader
    {
        private readonly IHttpService httpService;
        private readonly Dictionary<string, Sprite> cachedSprites;

        private readonly Queue<Action> spriteQueue;
        private readonly object loaderLock;
        private bool coroutineRunning;

        public SpriteLoader(IHttpService httpService)
        {
            this.httpService = httpService;
            cachedSprites = new Dictionary<string, Sprite>();

            spriteQueue = new Queue<Action>();
            loaderLock = new object();
            coroutineRunning = false;
        }

        public async void DownloadSpriteAsync(string spriteURL, Action<Sprite> onCompletion)
        {
            // Check Cache
            if (cachedSprites.TryGetValue(spriteURL, out Sprite cachedSprite))
            {
                onCompletion?.Invoke(cachedSprite);
                return;
            }

            try
            {
                IHttpResponse webResponse = await httpService.GetAsync(spriteURL, cancellationToken: CancellationToken.None).ConfigureAwait(false);
                byte[] imageBytes = await webResponse.ReadAsByteArrayAsync();
                QueueLoadSprite(spriteURL, imageBytes, onCompletion);
            }
            catch (Exception)
            {
                onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
            }
        }

        private void QueueLoadSprite(string spriteURL, byte[] imageBytes, Action<Sprite> onCompletion)
        {
            spriteQueue.Enqueue(() =>
            {
                try
                {
                    Sprite sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(imageBytes);
                    sprite.texture.wrapMode = TextureWrapMode.Clamp;
                    cachedSprites[spriteURL] = sprite;
                    onCompletion?.Invoke(sprite);
                }
                catch (Exception)
                {
                    onCompletion?.Invoke(BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite);
                }
            });

            if (!coroutineRunning)
            {
                IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(SpriteLoadCoroutine);
            }
        }

        public static YieldInstruction LoadWait = new WaitForEndOfFrame();

        private IEnumerator<YieldInstruction> SpriteLoadCoroutine()
        {
            lock (loaderLock)
            {
                if (coroutineRunning)
                    yield break;
                coroutineRunning = true;
            }

            while (spriteQueue.Count > 0)
            {
                yield return LoadWait;
                var loader = spriteQueue.Dequeue();
                loader?.Invoke();
            }

            coroutineRunning = false;
            if (spriteQueue.Count > 0)
            {
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
            }
        }
    }
}
