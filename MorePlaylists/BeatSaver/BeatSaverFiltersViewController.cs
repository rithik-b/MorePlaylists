using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using MorePlaylists.Utilities;
using UnityEngine;
using Zenject;

namespace MorePlaylists.BeatSaver
{
    [HotReload(RelativePathToLayout = @".\BeatSaverFiltersView.bsml")]
    [ViewDefinition("MorePlaylists.BeatSaver.BeatSaverFiltersView.bsml")]
    internal class BeatSaverFiltersViewController : BSMLAutomaticViewController
    {
        [Inject] 
        private readonly InputFieldGrabber inputFieldGrabber = null!;

        [Inject] 
        private readonly AnimationGrabber animationGrabber = null!;

        private InputFieldView? inputFieldView;
        private CurvedTextMeshPro? placeholderText;
        public readonly BeatSaverFilterModel filterOptions = new();

        public event Action<BeatSaverFilterModel>? FiltersSet;
        public event Action? RequestDismiss;

        [UIComponent("filters-tab-selector")] private readonly TextSegmentedControl? filtersTabSelector = null!;

        [UIComponent("filter-ui")] private readonly RectTransform? filterUITransform = null!;

        [UIComponent("search-vertical")] private readonly RectTransform? searchVerticalTransform = null!;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            inputFieldView = inputFieldGrabber.GetNewInputField(filterUITransform!, new Vector3(0, -15, 0));
            if (inputFieldView.transform is RectTransform inputFieldTransform)
            {
                inputFieldTransform.localPosition = new Vector3(-45, 19, 0);
                inputFieldTransform.sizeDelta = new Vector2(90, 12);
                inputFieldTransform.SetSiblingIndex(0);

                placeholderText = inputFieldTransform.Find("PlaceholderText").GetComponent<CurvedTextMeshPro>();
            }

            // For fixing no backgrounds in segmented control
            foreach (var cell in filtersTabSelector!.transform.GetComponentsInChildren<TextSegmentedControlCell>())
            {
                cell.transform.Find("BG").gameObject.SetActive(true);
            }
        }

        [UIAction("tab-switched")]
        private void TabSwitched(SegmentedControl _, int index)
        {
            if (index == 0)
            {
                searchVerticalTransform!.gameObject.SetActive(true);
                animationGrabber.PresentPanelAnimation.ExecuteAnimation(searchVerticalTransform!.gameObject);
                placeholderText!.text = "Search";
            }
            else
            {
                animationGrabber.DismissPanelAnimation.ExecuteAnimation(searchVerticalTransform!.gameObject,
                    () => searchVerticalTransform!.gameObject.SetActive(false));
                placeholderText!.text = "Enter BeatSaver username of user (must be exact)";
            }
        }

        [UIAction("cancel-click")]
        private void CancelClicked() => RequestDismiss?.Invoke();

        [UIAction("ok-click")]
        private void OkClicked()
        {
            if (filtersTabSelector!.selectedCellNumber == 0)
            {
                filterOptions.FilterMode = FilterMode.Search;
                filterOptions.SearchFilter.IncludeEmpty = IncludeEmpty;
                filterOptions.SearchFilter.IsCurated = CuratedOnly;
                filterOptions.SearchFilter.Query = inputFieldView!.text;
            }
            else
            {
                filterOptions.FilterMode = FilterMode.User;
                filterOptions.UserName = inputFieldView!.text;
            }

            FiltersSet?.Invoke(filterOptions);
            RequestDismiss?.Invoke();
        }

        public void RaiseFiltersSet() => FiltersSet?.Invoke(filterOptions);

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

        [UIValue("filters")]
        private readonly List<object> filters = new() {nameof(FilterMode.Search), nameof(FilterMode.User)};

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
}
