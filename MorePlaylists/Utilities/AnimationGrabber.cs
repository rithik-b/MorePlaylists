using HMUI;

namespace MorePlaylists.Utilities;

internal class AnimationGrabber
{
    private ColorsOverrideSettingsPanelController colorsPanel;
    
    private PanelAnimationSO? presentPanelAnimation;
    public PanelAnimationSO PresentPanelAnimation => presentPanelAnimation ??= Accessors.PresentAnimationAccessor(ref colorsPanel);
    
    private PanelAnimationSO? dismissPanelAnimation;
    public PanelAnimationSO DismissPanelAnimation => dismissPanelAnimation ??= Accessors.DismissAnimationAccessor(ref colorsPanel);
    
    public AnimationGrabber(GameplaySetupViewController gameplaySetupViewController)
    {
        colorsPanel = Accessors.ColorsPanelAccessor(ref gameplaySetupViewController);
    }
}
