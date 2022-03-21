using System.Linq;
using UnityEngine;

namespace MorePlaylists.Utilities;

public class MaterialGrabber
{
    private Material? noGlowRoundEdge;
    public Material NoGlowRoundEdge => noGlowRoundEdge ??= Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UINoGlowRoundEdge");
}
