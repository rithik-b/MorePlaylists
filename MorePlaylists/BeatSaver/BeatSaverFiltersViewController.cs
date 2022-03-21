using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Utilities;
using UnityEngine;
using Zenject;

namespace MorePlaylists.BeatSaver;

[HotReload(RelativePathToLayout = @".\BeatSaverFiltersView.bsml")]
[ViewDefinition("MorePlaylists.BeatSaver.BeatSaverFiltersView.bsml")]
internal class BeatSaverFiltersViewController : BSMLAutomaticViewController
{
    [Inject]
    private readonly InputFieldGrabber inputFieldGrabber = null!;
    
    private InputFieldView? inputFieldView;
    public readonly BeatSaverFilterModel filterOptions = new();
    
    public event Action<BeatSaverFilterModel>? FiltersSet;
    public event Action? RequestDismiss;
    
    [UIComponent("vertical")] 
    private readonly RectTransform? verticalTransform = null!;


    [UIAction("#post-parse")]
    private void PostParse()
    {
        inputFieldView = inputFieldGrabber.GetNewInputField(verticalTransform!, new Vector3(0, -15, 0));
        if (inputFieldView.transform is RectTransform inputFieldTransform)
        {
            inputFieldTransform.SetSiblingIndex(0);
            inputFieldTransform.sizeDelta = new Vector2(50, 8);
        }
    }
    
    [UIAction("cancel-click")]
    private void CancelClicked() => RequestDismiss?.Invoke();

    [UIAction("ok-click")]
    private void OkClicked()
    {
        filterOptions.SearchFilter.IncludeEmpty = IncludeEmpty;
        filterOptions.SearchFilter.IsCurated = CuratedOnly;

        if (inputFieldView != null)
        {
            filterOptions.SearchFilter.Query = inputFieldView.text;
        }

        FiltersSet?.Invoke(filterOptions);
    }

    public void ClearFilters()
    {
        filterOptions.ClearFilters();
        IncludeEmpty = false;
        CuratedOnly = false;
        
        if (inputFieldView != null)
        {
            inputFieldView.ClearInput();
        }
        
        FiltersSet?.Invoke(filterOptions);
    }
    
    private bool includeEmpty;
    [UIValue("include-empty")]
    private bool IncludeEmpty
    {
        get => includeEmpty;
        set
        {
            includeEmpty = value;
            NotifyPropertyChanged();
        }
    }

    private bool curatedOnly;
    [UIValue("curated-only")]
    private bool CuratedOnly
    {
        get => curatedOnly;
        set
        {
            curatedOnly = value;
            NotifyPropertyChanged();
        }
    }
}
