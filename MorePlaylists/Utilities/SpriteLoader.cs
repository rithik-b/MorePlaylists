using MorePlaylists.Entries;
using SiraUtil;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MorePlaylists.Utilities
{
    internal class SpriteLoader
    {
        private readonly SiraClient siraClient;
        private readonly Dictionary<string, Sprite> cachedURLSprites;
        private readonly Dictionary<string, Sprite> cachedBase64Sprites;

        private readonly Queue<Action> spriteQueue;
        private readonly object loaderLock;
        private bool coroutineRunning;

        public SpriteLoader(SiraClient siraClient)
        {
            this.siraClient = siraClient;
            cachedURLSprites = new Dictionary<string, Sprite>();
            cachedBase64Sprites = new Dictionary<string, Sprite>();

            spriteQueue = new Queue<Action>();
            loaderLock = new object();
            coroutineRunning = false;
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
                WebResponse webResponse = await siraClient.GetAsync(spriteURL, CancellationToken.None).ConfigureAwait(false);
                byte[] imageBytes = webResponse.ContentToBytes();
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

            if (!coroutineRunning)
            {
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
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
