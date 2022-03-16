using System.Threading.Tasks;
using MorePlaylists.Sources;
using Newtonsoft.Json;
using SiraUtil.Web;

namespace MorePlaylists.Entries
{
    internal class AccSaberEntry : SongDetailsEntry
    {
        [JsonProperty("displayName")]
        public override string Title { get; protected set; }
        
        public override string Author
        {
            get => nameof(AccSaber);
            protected set {}
        }
        
        [JsonProperty("description")]
        public override string Description { get; protected set; }
        
        [JsonProperty("downloadLink")]
        public override string PlaylistURL { get; protected set; }
        
        [JsonProperty("imageUrl")]
        public override string SpriteString { get; protected set; }
        
        public override SpriteType SpriteType => SpriteType.URL;
    }
}
