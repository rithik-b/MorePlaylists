using System.IO;
using UnityEngine;

namespace MorePlaylists.Types
{
    public interface IGenericEntry
    {
        string Title { get; }
        string Author { get; }
        string Description { get; }
        string PlaylistURL { get; }
        Sprite Sprite { get; }
        Stream GetCoverStream();
    }
}
