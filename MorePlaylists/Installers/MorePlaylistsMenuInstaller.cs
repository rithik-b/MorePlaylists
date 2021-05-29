using HMUI;
using MorePlaylists.UI;
using SiraUtil;
using Zenject;

namespace MorePlaylists.Installers
{
    public class MorePlaylistsMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<MorePlaylistsListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<MorePlaylistsNavigationController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<MorePlaylistsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonUI>().AsSingle();
        }
    }
}
