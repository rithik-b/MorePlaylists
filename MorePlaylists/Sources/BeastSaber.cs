using UnityEngine;
using MorePlaylists.Entries;
using SiraUtil.Web;

namespace MorePlaylists.Sources
{
    internal class BeastSaber : LocalSearchSource<BeastSaberEntry>
    {
        private readonly IHttpService _siraHttpService;
        private Sprite _logo;

        public override string Website => "https://bsaber.com/";
        public override string Endpoint => "PlaylistAPI/playlistAPI.json";
        protected override IHttpService SiraHttpService => _siraHttpService;

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

        public BeastSaber(IHttpService siraHttpService)
        {
            _siraHttpService = siraHttpService;
        }
    }
}
