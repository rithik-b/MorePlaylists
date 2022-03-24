using MorePlaylists.Entries;
using Newtonsoft.Json;

namespace MorePlaylists.Hitbloq
{
    internal class HitbloqEntry : SongDetailsEntry
    {
        [JsonProperty("title")] 
        public override string Title { get; protected set; } = "";

        [JsonProperty("author")]
        public override string Author { get; protected set; } = "";

        [JsonProperty("description")]
        public override string Description { get; protected set; } = "";
        
        [JsonProperty("download_url")] 
        public override string PlaylistURL { get; protected set; } = null!;

        [JsonProperty("image")]
        public override string SpriteURL { get; protected set; } = null!;
        
        public override string FolderName => "Hitbloq";
    }
}
