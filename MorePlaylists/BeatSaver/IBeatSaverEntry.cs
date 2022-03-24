using BeatSaverSharp.Models;
using MorePlaylists.Entries;

namespace MorePlaylists.BeatSaver
{
    internal interface IBeatSaverEntry : IEntry
    {
        User Owner { get; }
    }
}
