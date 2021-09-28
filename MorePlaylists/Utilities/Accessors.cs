using HMUI;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MorePlaylists.Utilities
{
    internal class Accessors
    {
        #region ScrollView

        public static readonly FieldAccessor<ScrollView, Button>.Accessor PageUpAccessor = FieldAccessor<ScrollView, Button>.GetAccessor("_pageUpButton");

        public static readonly FieldAccessor<ScrollView, Button>.Accessor PageDownAccessor = FieldAccessor<ScrollView, Button>.GetAccessor("_pageDownButton");

        public static readonly FieldAccessor<ScrollView, VerticalScrollIndicator>.Accessor ScrollIndicatorAccessor =
            FieldAccessor<ScrollView, VerticalScrollIndicator>.GetAccessor("_verticalScrollIndicator");

        public static readonly FieldAccessor<ScrollView, IVRPlatformHelper>.Accessor PlatformHelperAccessor =
            FieldAccessor<ScrollView, IVRPlatformHelper>.GetAccessor("_platformHelper");

        public static readonly FieldAccessor<ScrollView, RectTransform>.Accessor ScrollViewportAccessor =
            FieldAccessor<ScrollView, RectTransform>.GetAccessor("_viewport");

        public static readonly FieldAccessor<ScrollView, RectTransform>.Accessor ScrollContentAccessor =
            FieldAccessor<ScrollView, RectTransform>.GetAccessor("_contentRectTransform");

        #endregion

        public static readonly FieldAccessor<StandardLevelDetailViewController, LoadingControl>.Accessor LoadingControlAccessor =
            FieldAccessor<StandardLevelDetailViewController, LoadingControl>.GetAccessor("_loadingControl");
    }
}
