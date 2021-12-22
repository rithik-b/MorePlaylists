using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp.Models;
using Newtonsoft.Json;
using SiraUtil.Web;

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

        private List<Song> songs;

        public override async Task<List<Song>> GetSongs(IHttpService siraHttpService)
        {
            if (songs == null)
            {
                songs = new List<Song>();
                int page = 0;
                bool limitReached = false;
                while (!limitReached)
                {
                    try
                    {
                        IHttpResponse webResponse = await siraHttpService.GetAsync($"https://api.beatsaver.com/playlists/id/{PlaylistID}/{page}", cancellationToken: CancellationToken.None);
                        if (webResponse.Successful)
                        {
                            BeatSaverDetailEntry detailEntry = JsonConvert.DeserializeObject<BeatSaverDetailEntry>(await webResponse.ReadAsStringAsync());
                            if (detailEntry.maps.Count != 0)
                            {
                                foreach (BeatSaverMapDetailWithOrder map in detailEntry.maps)
                                {
                                    songs.Add(new Song(map.map.Metadata.SongName, $"{map.map.Metadata.SongAuthorName} [{map.map.Metadata.LevelAuthorName}]", map.map.LatestVersion.CoverURL));
                                }
                            }
                            else
                            {
                                limitReached = true;
                            }
                            page++;
                        }
                        else
                        {
                            limitReached = true;
                        }
                    }
                    catch (Exception)
                    {
                        limitReached = true;
                    }
                }
            }
            return songs;
        }
    }

    internal class BeatSaverAuthor
    {
        [JsonProperty("name")]
        public string Name { get; protected set; }
    }

    internal class BeatSaverDetailEntry
    {
        [JsonProperty("maps")]
        public List<BeatSaverMapDetailWithOrder> maps { get; protected set; }
    }

    internal class BeatSaverMapDetailWithOrder
    {
        [JsonProperty("map")]
        public Beatmap map { get; protected set; }

        [JsonProperty("order")]
        public float order { get; protected set; }
    }
}
