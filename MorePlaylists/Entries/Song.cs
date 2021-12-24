namespace MorePlaylists.Entries
{
    public class Song
    {
        public virtual string Name { get; protected set; }
        public virtual string SubName { get; protected set; }
        public virtual string CoverURL { get; protected set; }

        public Song(string name, string subName, string coverURL)
        {
            Name = name;
            SubName = subName;
            CoverURL = coverURL;
        }

        public Song() { }
    }
}
