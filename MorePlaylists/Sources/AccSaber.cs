using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MorePlaylists.Entries;
using SiraUtil.Web;
using UnityEngine;

namespace MorePlaylists.Sources
{
    internal class AccSaber : SinglePageSource<AccSaberEntry>
    {
        private readonly IHttpService siraHttpService;
        private Sprite _logo;
        
        public override string Website => "https://api.accsaber.com/";
        public override string Endpoint => "playlists/";
        protected override IHttpService SiraHttpService => siraHttpService;
        
        public override Sprite Logo
        {
            get
            {
                if (_logo == null)
                {
                    _logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.AccSaber.png");
                }
                return _logo;
            }
        }

        public AccSaber(IHttpService siraHttpService)
        {
            this.siraHttpService = siraHttpService;
        }
    }
}
