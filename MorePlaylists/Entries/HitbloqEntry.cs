using Newtonsoft.Json;

namespace MorePlaylists.Entries
{
    public class HitbloqEntry : ImageFileEntry
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
        protected override string CoverURL
        {
            get => base.CoverURL;
            set => base.CoverURL = value;
        }
    }
}
