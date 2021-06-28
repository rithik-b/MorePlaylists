using Newtonsoft.Json;

namespace MorePlaylists.Entries
{
    public class BeastSaberEntry : Base64Entry
    {
        [JsonProperty("playlistTitle")]
        public override string Title { get; protected set; }

        [JsonProperty("playlistAuthor")]
        public override string Author { get; protected set; }

        [JsonProperty("playlistDescription")]
        private string BSaberDescription { get; set; }

        [JsonProperty("playlistCategory")]
        private string Category { get; set; }

        [JsonProperty("playlistURL")]
        public override string PlaylistURL { get; protected set; }

        [JsonProperty("image")]
        protected override string CoverString
        {
            get => base.CoverString;
            set => base.CoverString = value;
        }

        public override string Description
        {
            get => $"Category: {Category}\n\n{BSaberDescription}";
            protected set { }
        }
    }
}
