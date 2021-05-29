using BeatSaverSharp;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MorePlaylists.Utilities
{
    internal class DownloaderUtils
    {
        private BeatSaver beatSaverInstance;
        public static DownloaderUtils instance;
        public static void Init()
        {
            instance = new DownloaderUtils();
            HttpOptions options = new HttpOptions(name: typeof(DownloaderUtils).Assembly.GetName().Name, version: typeof(DownloaderUtils).Assembly.GetName().Version);
            instance.beatSaverInstance = new BeatSaver(options);
        }

        public async Task<byte[]> DownloadFileToBytesAsync(string url, CancellationToken token)
        {
            Uri uri = new Uri(url);
            using (var webClient = new WebClient())
            using (var registration = token.Register(() => webClient.CancelAsync()))
            {
                var data = await webClient.DownloadDataTaskAsync(uri);
                return data;
            }
        }
    }
}
