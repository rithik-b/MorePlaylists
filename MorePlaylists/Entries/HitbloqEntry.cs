using Newtonsoft.Json;

namespace MorePlaylists.Entries
{
    public class HitbloqEntry : GenericEntry
    {
        [JsonProperty("title")]
        public override string Title { get; protected set; }

        [JsonProperty("author")]
        public override string Author { get; protected set; }

        [JsonProperty("description")]
        public override string Description { get; protected set; }

        [JsonProperty("download_url")]
        public override string PlaylistURL { get; protected set; }

        [JsonProperty("image")]
        public override string SpriteString { get; protected set; }

        public override SpriteType SpriteType => SpriteType.URL;
    }
}
