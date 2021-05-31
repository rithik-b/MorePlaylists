using Newtonsoft.Json;

namespace MorePlaylists.Types
{
    public class BSaberEntry : Base64Entry
    {
        [JsonProperty("playlistTitle")]
        public override string Title { get; protected set; }

        [JsonProperty("playlistAuthor")]
        public override string Author { get; protected set; }

        [JsonProperty("playlistDescription")]
        public override string Description { get; protected set; }

        [JsonProperty("playlistURL")]
        public override string PlaylistURL { get; protected set; }

        [JsonProperty("image")]
        protected override string CoverString
        {
            get => base.CoverString;
            set => base.CoverString = value;
        }
    }
}
