﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace MorePlaylists.Entries
{
    internal class BeatSaverResponse
    {
        [JsonProperty("docs")]
        public List<BeatSaverEntry> Entries { get; protected set; }
    }

    internal class BeatSaverEntry : GenericEntry
    {
        [JsonProperty("name")]
        public override string Title { get; protected set; }

        [JsonProperty("owner")]
        public BeatSaverAuthor Owner { get; protected set; }

        [JsonProperty("description")]
        public override string Description { get; protected set; }

        [JsonProperty("playlistId")]
        public string PlaylistID { get; protected set; }

        [JsonProperty("playlistImage")]
        public override string SpriteString { get; protected set; }

        public override SpriteType SpriteType => SpriteType.URL;

        public override string Author 
        { 
            get => Owner.Name;
            protected set { } 
        }

        public override string PlaylistURL
        {
            get => $"https://api.beatsaver.com/playlists/id/{PlaylistID}/download";
            protected set { }
        }
    }

    internal class BeatSaverAuthor
    {
        [JsonProperty("name")]
        public string Name { get; protected set; }
    }
}