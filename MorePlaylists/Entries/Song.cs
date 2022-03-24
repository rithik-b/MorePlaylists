namespace MorePlaylists.Entries
{
    internal class Song
    {
        public string Name { get; }
        public string SubName { get; }
        public string CoverURL { get; }

        public Song(string name, string subName, string coverURL)
        {
            Name = name;
            SubName = subName;
            CoverURL = coverURL;
        }
    }
}
