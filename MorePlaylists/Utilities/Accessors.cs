using HMUI;
using IPA.Utilities;

namespace MorePlaylists.Utilities
{
    internal class Accessors
    {
        public static readonly FieldAccessor<StandardLevelDetailViewController, LoadingControl>.Accessor LoadingControlAccessor =
            FieldAccessor<StandardLevelDetailViewController, LoadingControl>.GetAccessor("_loadingControl");

        public static readonly FieldAccessor<ModalView, bool>.Accessor AnimateCanvasAccessor = FieldAccessor<ModalView, bool>.GetAccessor("_animateParentCanvas");
    }
}
