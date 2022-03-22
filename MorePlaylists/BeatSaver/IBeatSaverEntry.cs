using BeatSaverSharp.Models;
using MorePlaylists.Entries;

namespace MorePlaylists.BeatSaver;

public interface IBeatSaverEntry : IEntry
{
    User Owner { get; }
}
