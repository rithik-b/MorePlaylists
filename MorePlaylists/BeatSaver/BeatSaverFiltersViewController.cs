using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaverSharp;
using HMUI;
using MorePlaylists.Utilities;
using UnityEngine;
using Zenject;

namespace MorePlaylists.BeatSaver;

[HotReload(RelativePathToLayout = @".\BeatSaverFiltersView.bsml")]
[ViewDefinition("MorePlaylists.BeatSaver.BeatSaverFiltersView.bsml")]
public class BeatSaverFiltersViewController : BSMLAutomaticViewController
{
    [Inject]
    private readonly InputFieldGrabber inputFieldGrabber = null!;
    
    private InputFieldView? inputFieldView;

    public SearchTextPlaylistFilterOptions? filterOptions { get; private set; }
    private SearchTextPlaylistFilterOptions FilterOptions => filterOptions ??= new SearchTextPlaylistFilterOptions();

    public event Action<SearchTextPlaylistFilterOptions?>? FiltersSet;
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
        FilterOptions.IncludeEmpty = IncludeEmpty;
        FilterOptions.IsCurated = CuratedOnly;

        if (inputFieldView != null)
        {
            FilterOptions.Query = inputFieldView.text;
        }

        FiltersSet?.Invoke(FilterOptions);
    }

    public void ClearFilters()
    {
        filterOptions = null;
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
