using MorePlaylists.Entries;
using Newtonsoft.Json;
using SiraUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MorePlaylists.Sources
{
    internal class Hitbloq : LocalSearchSource<HitbloqEntry>
    {
        private SiraClient _siraClient;
        private Sprite _logo;

        public override string Website => "https://hitbloq.com/";
        public override string Endpoint => "api/map_pools_detailed";
        protected override SiraClient SiraClient => _siraClient;

        public override Sprite Logo
        {
            get
            {
                if (_logo == null)
                {
                    _logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.Hitbloq.png");
                }
                return _logo;
            }
        }

        public Hitbloq(SiraClient siraClient)
        {
            _siraClient = siraClient;
        }
    }
}
