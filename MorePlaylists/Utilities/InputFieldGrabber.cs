using HMUI;
using UnityEngine;
using UnityEngine.UI;

namespace MorePlaylists.Utilities;

internal class InputFieldGrabber
{
    private LevelSearchViewController levelSearchViewController;
    
    private InputFieldView? inputFieldTemplate;
    private InputFieldView InputFieldTemplate =>
        inputFieldTemplate ??= Accessors.InputFieldAccessor(ref levelSearchViewController);

    private Button? filtersButtonTemplate;
    private Button FiltersButtonTemplate =>
        filtersButtonTemplate ??= Accessors.FiltersButtonAccessor(ref levelSearchViewController);

    public InputFieldGrabber(LevelSearchViewController levelSearchViewController)
    {
        this.levelSearchViewController = levelSearchViewController;
    }

    public InputFieldView GetNewInputField(Transform parent, Vector3 offset)
    {
        var newInputField = Object.Instantiate(InputFieldTemplate, parent, false);
        Accessors.KeyboardOffsetAccessor(ref newInputField) = offset;
        return newInputField;
    }

    public Button GetNewFiltersButton(Transform parent)
    {
        var newFiltersButton = Object.Instantiate(FiltersButtonTemplate, parent, false);
        return newFiltersButton;
    }
}
