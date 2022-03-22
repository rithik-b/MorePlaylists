using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using BeatSaverSharp.Models;
using BeatSaverSharp.Models.Pages;
using MorePlaylists.Entries;
using SiraUtil.Web;

namespace MorePlaylists.BeatSaver;

internal class BeatSaverUserPlaylistEntry : IBeatSaverEntry
{
    public string Title => $"Maps by {Owner.Name}";
    public string Author => Owner.Name;
    public string Description => $"All maps by {Owner.Name}";
    public string PlaylistURL => $"https://api.beatsaver.com/users/id/{Owner.ID}/playlist";
    public string SpriteURL => Owner.Avatar;
    public IPlaylist? LocalPlaylist { get; set; }
    public bool DownloadBlocked { get; set; }
    public bool ExhaustedPages { get; private set; }
    public User Owner { get; }

    private Page? beatmapsPage;
    
    public BeatSaverUserPlaylistEntry(User owner)
    {
        Owner = owner;
    }
    
    public async Task<List<Song>?> GetSongs(IHttpService siraHttpService, CancellationToken cancellationToken = default, bool firstPage = false)
    {
        if (beatmapsPage == null || firstPage)
        {
            beatmapsPage = await Owner.Beatmaps(token: cancellationToken);
            ExhaustedPages = false;
        }
        else
        {
            var newPage = await beatmapsPage.Next(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            beatmapsPage = newPage;
        }

        if (beatmapsPage == null || beatmapsPage.Empty)
        {
            ExhaustedPages = true;
            return null;
        }

        var songs = new List<Song>();
        foreach (var beatmap in beatmapsPage.Beatmaps)
        {
            songs.Add(new Song(beatmap.Name, $"{beatmap.Metadata.SongAuthorName} [{beatmap.Metadata.LevelAuthorName}]", beatmap.LatestVersion.CoverURL));
        }
        return songs;
    }

    public async Task<IPlaylist?> DownloadPlaylist(IHttpService siraHttpService, CancellationToken cancellationToken = default)
    {
        try
        {
            var webResponse = await siraHttpService.GetAsync(PlaylistURL, cancellationToken: cancellationToken);
            if (webResponse.Successful)
            {
                using var playlistStream = await webResponse.ReadAsStreamAsync();
                var playlist = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.DefaultHandler?.Deserialize(playlistStream);
                return playlist;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                Plugin.Log?.Error("An error occurred while acquiring " + PlaylistURL +
                                  $"\nError code: {webResponse.Code}");
            }
        }
        catch (Exception e)
        {
            if (e is not TaskCanceledException)
            {
                Plugin.Log?.Error("An exception occurred while acquiring " + PlaylistURL +
                                  $"\nException: {e.Message}");
            }
        }
        return null;
    }
}
