using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using MorePlaylists.Sources;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace MorePlaylists.UI
{
    public class SourceModalController : IInitializable
    {
        private List<ISource> sources;
        private bool parsed;
        internal event Action<ISource> DidSelectSource;

        [UIComponent("list")]
        private CustomListTableData customListTableData;

        [UIComponent("source-modal")]
        private ModalView sourceModalView;

        [UIComponent("source-modal")]
        private readonly RectTransform sourceModalTransform;

        private Vector3 sourceModalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        [Inject]
        public void Construct(List<ISource> sources)
        {
            this.sources = sources;
        }

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
            foreach (ISource source in sources)
            {
                customListTableData.data.Add(new CustomListTableData.CustomCellInfo(source.GetType().Name, "", source.Logo));
            }
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
            DidSelectSource?.Invoke(sources[row]);
            parserParams.EmitEvent("close-source-modal");
        }
    }
}
