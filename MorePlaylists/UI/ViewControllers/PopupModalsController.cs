using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using MorePlaylists.Utilities;
using PlaylistManager.Types;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace MorePlaylists.UI
{
    internal class PopupModalsController : INotifyPropertyChanged
    {
        private readonly MorePlaylistsListViewController morePlaylistsListViewController;
        private bool parsed;
        public event PropertyChangedEventHandler? PropertyChanged;
        
        private Action? okButtonPressed;
        private string okText = "";
        private string okButtonText = "Ok";

        [UIComponent("root")]
        private readonly RectTransform rootTransform = null!;

        [UIComponent("ok-modal")]
        private readonly RectTransform okModalTransform = null!;

        [UIComponent("ok-modal")]
        private ModalView okModalView = null!;

        private Vector3? okModalPosition;
        
        [UIParams]
        private readonly BSMLParserParams parserParams = null!;

        public PopupModalsController(MorePlaylistsListViewController morePlaylistsListViewController)
        {
            this.morePlaylistsListViewController = morePlaylistsListViewController;
        }

        private void Parse()
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "MorePlaylists.UI.Views.PopupModals.bsml"), morePlaylistsListViewController.gameObject, this);
                okModalPosition = okModalTransform.localPosition;
                parsed = true;
            }
        }

        #region Ok Modal

        // Methods

        private void ShowOkModal(OkPopupContents popupContents)
        {
            ShowOkModal(popupContents.parent, popupContents.message, popupContents.buttonPressedCallback, popupContents.okButtonText, popupContents.animateParentCanvas);
        }

        internal void ShowOkModal(Transform parent, string text, Action? buttonPressedCallback, string okButtonText = "Ok", bool animateParentCanvas = true)
        {
            Parse();
            okModalTransform.localPosition = okModalPosition!.Value;
            okModalTransform.transform.SetParent(parent);

            OkText = text;
            OkButtonText = okButtonText;
            okButtonPressed = buttonPressedCallback;

            Accessors.AnimateCanvasAccessor(ref okModalView) = animateParentCanvas;

            parserParams.EmitEvent("close-ok");
            parserParams.EmitEvent("open-ok");
        }

        [UIAction("ok-button-pressed")]
        private void OkButtonPressed()
        {
            okButtonPressed?.Invoke();
            okButtonPressed = null;
            okModalTransform.transform.SetParent(rootTransform);
        }

        // Values

        [UIValue("ok-text")]
        internal string OkText
        {
            get => okText;
            set
            {
                okText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OkText)));
            }
        }

        [UIValue("ok-button-text")]
        internal string OkButtonText
        {
            get => okButtonText;
            set
            {
                okButtonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OkButtonText)));
            }
        }

        #endregion
    }
}
