using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using MorePlaylists.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    public class SourceModalController : IInitializable
    {
        private bool parsed;
        internal event Action<DownloadSource> DidSelectSource;

        [UIComponent("list")]
        private CustomListTableData customListTableData;

        [UIComponent("source-modal")]
        private ModalView sourceModalView;

        [UIComponent("source-modal")]
        private readonly RectTransform sourceModalTransform;

        private Vector3 sourceModalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public void Initialize()
        {
            parsed = false;
        }

        private void Parse(Transform parent)
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "MorePlaylists.UI.Views.SourceModal.bsml"), parent.gameObject, this);
                sourceModalPosition = sourceModalTransform.position;
                parsed = true;
            }
            sourceModalTransform.SetParent(parent);
            sourceModalTransform.localPosition = sourceModalPosition;
            FieldAccessor<ModalView, bool>.Set(ref sourceModalView, "_animateParentCanvas", true);
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            customListTableData.data.Clear();
            customListTableData.data.Add(new CustomListTableData.CustomCellInfo("BeastSaber", "", BSaberUtils.LOGO));
            customListTableData.data.Add(new CustomListTableData.CustomCellInfo("Hitbloq", "", HitbloqUtils.LOGO));
            customListTableData.tableView.ReloadData();
            customListTableData.tableView.SelectCellWithIdx(0);
        }

        internal void ShowModal(Transform parent)
        {
            Parse(parent);
            parserParams.EmitEvent("close-source-modal");
            parserParams.EmitEvent("open-source-modal");
        }

        [UIAction("list-select")]
        private void Select(TableView tableView, int row)
        {
            DidSelectSource?.Invoke((DownloadSource)Enum.ToObject(typeof(DownloadSource), row));
            parserParams.EmitEvent("close-source-modal");
        }
    }
    public enum DownloadSource { BSaber, Hitbloq };
}
