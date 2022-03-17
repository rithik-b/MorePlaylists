using MorePlaylists.Sources;
using MorePlaylists.UI;
using SiraUtil.Web;
using UnityEngine;

namespace MorePlaylists.AccSaber
{
    internal class AccSaber : BasicSource<AccSaberEntry>
    {
        private Sprite? logo;
        protected override string Website => "https://api.accsaber.com/";
        protected override string Endpoint => "playlists/";
        public override Sprite Logo
        {
            get
            {
                if (logo == null)
                {
                    logo = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("MorePlaylists.Images.AccSaber.png");
                }
                return logo;
            }
        }

        public AccSaber(IHttpService siraHttpService, MorePlaylistsListViewController listViewController, MorePlaylistsDetailViewController detailViewController) : base(siraHttpService, listViewController, detailViewController)
        {
        }
    }
}
