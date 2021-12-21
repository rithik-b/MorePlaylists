namespace MorePlaylists.Entries
{
    public class Song
    {
        public readonly string name;
        public readonly string subName;
        public readonly string coverURL;

        public Song(string name, string subName, string coverURL)
        {
            this.name = name;
            this.subName = subName;
            this.coverURL = coverURL;
        }
    }
}
