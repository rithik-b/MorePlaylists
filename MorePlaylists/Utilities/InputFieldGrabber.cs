using HMUI;
using UnityEngine;

namespace MorePlaylists.Utilities;

internal class InputFieldGrabber
{
    private LevelSearchViewController levelSearchViewController;
    
    private InputFieldView? inputFieldTemplate;
    private InputFieldView InputFieldTemplate =>
        inputFieldTemplate ??= Accessors.InputFieldAccessor(ref levelSearchViewController);

    public InputFieldGrabber(LevelSearchViewController levelSearchViewController)
    {
        this.levelSearchViewController = levelSearchViewController;
    }

    public InputFieldView GetNewInputField(Transform parent)
    {
        var newInputField = Object.Instantiate(InputFieldTemplate, parent, false);
        Accessors.KeyboardOffsetAccessor(ref newInputField) = new Vector3(0, -30, 0);
        return newInputField;
    }
}
