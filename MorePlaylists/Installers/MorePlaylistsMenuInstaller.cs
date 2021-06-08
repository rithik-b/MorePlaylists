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
            Container.Bind<MorePlaylistsSongListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MorePlaylistsDownloadQueueViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MorePlaylistsDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<MorePlaylistsNavigationController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PopupModalsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<MorePlaylistsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonUI>().AsSingle();
        }
    }
}
