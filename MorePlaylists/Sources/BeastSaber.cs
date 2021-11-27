using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SiraUtil;
using MorePlaylists.Entries;

namespace MorePlaylists.Sources
{
    internal class BeastSaber : LocalSearchSource<BeastSaberEntry>
    {
        private SiraClient _siraClient;
        private Sprite _logo;

        public override string Website => "https://bsaber.com/";
        public override string Endpoint => "PlaylistAPI/playlistAPI.json";
        protected override SiraClient SiraClient => _siraClient;

        public override Sprite Logo
        {
            get
            {
                if (_logo == null)
                {
                    _logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.BeastSaber.png");
                }
                return _logo;
            }
        }

        public BeastSaber(SiraClient siraClient)
        {
            _siraClient = siraClient;
        }
    }
}
