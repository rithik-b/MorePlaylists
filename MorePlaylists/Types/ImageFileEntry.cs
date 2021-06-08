using MorePlaylists.Utilities;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MorePlaylists.Types
{
    public abstract class ImageFileEntry : GenericEntry
    {
        protected virtual string CoverURL { get; set; }
        protected byte[] imageData = null;
        public override Stream GetCoverStream() => new MemoryStream(imageData);
        internal async Task DownloadImage(CancellationToken token)
        {
            imageData = await DownloaderUtils.instance.DownloadFileToBytesAsync(CoverURL, token);
        }
    }
}
