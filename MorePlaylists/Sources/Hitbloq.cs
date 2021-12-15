using MorePlaylists.Entries;
using SiraUtil.Web;
using UnityEngine;

namespace MorePlaylists.Sources
{
    internal class Hitbloq : LocalSearchSource<HitbloqEntry>
    {
        private IHttpService _siraHttpService;
        private Sprite _logo;

        public override string Website => "https://hitbloq.com/";
        public override string Endpoint => "api/map_pools_detailed";
        protected override IHttpService SiraHttpService => _siraHttpService;

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

        public Hitbloq(IHttpService siraHttpService)
        {
            _siraHttpService = siraHttpService;
        }
    }
}
