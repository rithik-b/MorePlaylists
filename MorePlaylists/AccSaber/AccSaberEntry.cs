using MorePlaylists.Entries;
using Newtonsoft.Json;

namespace MorePlaylists.AccSaber
{
    internal class AccSaberEntry : SongDetailsEntry
    {
        [JsonProperty("displayName")] 
        public override string Title { get; protected set; } = "";
        
        public override string Author { get; protected set; } = "AccSaber";
        
        [JsonProperty("description")]
        public override string Description { get; protected set; } = "";

        [JsonProperty("downloadLink")] 
        public override string PlaylistURL { get; protected set; } = null!;
        
        [JsonProperty("imageUrl")]
        public override string SpriteURL { get; protected set; } = null!;
        public override string FolderName => "AccSaber";
    }
}
