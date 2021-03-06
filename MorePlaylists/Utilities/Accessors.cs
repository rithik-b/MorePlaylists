using HMUI;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MorePlaylists.Utilities
{
    internal class Accessors
    {
        public static readonly FieldAccessor<TableView, ScrollView>.Accessor ScrollViewAccessor = FieldAccessor<TableView, ScrollView>.GetAccessor("_scrollView");

        public static readonly FieldAccessor<ScrollView, Button>.Accessor PageDownAccessor = FieldAccessor<ScrollView, Button>.GetAccessor("_pageDownButton");

        public static readonly FieldAccessor<StandardLevelDetailViewController, LoadingControl>.Accessor LoadingControlAccessor =
            FieldAccessor<StandardLevelDetailViewController, LoadingControl>.GetAccessor("_loadingControl");

        public static readonly FieldAccessor<ModalView, bool>.Accessor AnimateCanvasAccessor = FieldAccessor<ModalView, bool>.GetAccessor("_animateParentCanvas");

        public static readonly FieldAccessor<LevelSelectionFlowCoordinator.State, SelectLevelCategoryViewController.LevelCategory?>.Accessor LevelCategoryAccessor =
            FieldAccessor<LevelSelectionFlowCoordinator.State, SelectLevelCategoryViewController.LevelCategory?>.GetAccessor("levelCategory");
        
        public static readonly FieldAccessor<LevelSearchViewController, InputFieldView>.Accessor InputFieldAccessor =
            FieldAccessor<LevelSearchViewController, InputFieldView>.GetAccessor("_searchTextInputFieldView");
        
        public static readonly FieldAccessor<LevelSearchViewController, Button>.Accessor FiltersButtonAccessor =
            FieldAccessor<LevelSearchViewController, Button>.GetAccessor("_searchButton");
        
        public static readonly FieldAccessor<InputFieldView, Vector3>.Accessor KeyboardOffsetAccessor =
            FieldAccessor<InputFieldView, Vector3>.GetAccessor("_keyboardPositionOffset");
        
        public static readonly FieldAccessor<GameplaySetupViewController, ColorsOverrideSettingsPanelController>.Accessor ColorsPanelAccessor =
            FieldAccessor<GameplaySetupViewController, ColorsOverrideSettingsPanelController>.GetAccessor("_colorsOverrideSettingsPanelController");

        public static readonly FieldAccessor<ColorsOverrideSettingsPanelController, PanelAnimationSO>.Accessor PresentAnimationAccessor =
            FieldAccessor<ColorsOverrideSettingsPanelController, PanelAnimationSO>.GetAccessor("_presentPanelAnimation");

        public static readonly FieldAccessor<ColorsOverrideSettingsPanelController, PanelAnimationSO>.Accessor DismissAnimationAccessor =
            FieldAccessor<ColorsOverrideSettingsPanelController, PanelAnimationSO>.GetAccessor("_dismissPanelAnimation");
    }
}
