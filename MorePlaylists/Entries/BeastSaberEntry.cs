﻿using Newtonsoft.Json;

namespace MorePlaylists.Entries
{
    internal class BeastSaberEntry : GenericEntry
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
        public override string SpriteString { get; protected set; }
        public override SpriteType SpriteType => SpriteType.Base64;

        public override string Description
        {
            get => $"Category: {Category}\n\n{BSaberDescription}";
            protected set { }
        }
    }
}
