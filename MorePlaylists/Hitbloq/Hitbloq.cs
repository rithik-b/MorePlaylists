using MorePlaylists.Entries;
using MorePlaylists.Sources;
using MorePlaylists.UI;
using SiraUtil.Web;
using UnityEngine;

namespace MorePlaylists.Hitbloq
{
    internal class Hitbloq : BasicSource<HitbloqEntry>
    {
        private Sprite? logo;
        protected override string Website => "https://hitbloq.com/";
        protected override string Endpoint => "api/map_pools_detailed";
        public override Sprite Logo
        {
            get
            {
                if (logo == null)
                {
                    logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.Hitbloq.png");
                }
                return logo;
            }
        }
        public Hitbloq(IHttpService siraHttpService, MorePlaylistsListViewController listViewController, MorePlaylistsDetailViewController detailViewController) : base(siraHttpService, listViewController, detailViewController)
        {
        }
    }
}
