using System.Threading.Tasks;
using SiraUtil.Web;

namespace MorePlaylists.Entries
{
    internal class AccSaberEntry : SongDetailsEntry
    {
        public override string Title { get; protected set; }
        public override string Author { get; protected set; }
        public override string Description { get; protected set; }
        public override string PlaylistURL { get; protected set; }
        public override string SpriteString { get; protected set; }
        public override SpriteType SpriteType => SpriteType.Playlist;

        public static async Task<AccSaberEntry> GetAccSaberPlaylist(string playlistURL, IHttpService siraHttpService)
        {
            var accSaberEntry = new AccSaberEntry();
            accSaberEntry.PlaylistURL = playlistURL;
            await accSaberEntry.DownloadPlaylist(siraHttpService);
            if (accSaberEntry.RemotePlaylist != null)
            {
                accSaberEntry.Title = accSaberEntry.RemotePlaylist.Title;
                accSaberEntry.Author = accSaberEntry.RemotePlaylist.Author;
                accSaberEntry.Description = accSaberEntry.RemotePlaylist.Description;
                return accSaberEntry;
            }
            return null;
        }
    }
}
