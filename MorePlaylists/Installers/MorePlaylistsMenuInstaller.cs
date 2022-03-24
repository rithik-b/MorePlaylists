using MorePlaylists.BeatSaver;
using MorePlaylists.UI;
using MorePlaylists.Utilities;
using Zenject;

namespace MorePlaylists.Installers
{
    internal class MorePlaylistsMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<BeatSaver.BeatSaver>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatSaverListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatSaverDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatSaverFiltersViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesTo<Hitbloq.Hitbloq>().AsSingle();
            Container.BindInterfacesTo<AccSaber.AccSaber>().AsSingle();

            Container.BindInterfacesAndSelfTo<BasicListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MorePlaylistsSongListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MorePlaylistsDownloaderViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BasicDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<MorePlaylistsNavigationController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesAndSelfTo<PopupModalsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SourceModalController>().AsSingle();

            Container.BindInterfacesAndSelfTo<MorePlaylistsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonUI>().AsSingle();

            Container.BindInterfacesAndSelfTo<SpriteLoader>().AsSingle();
            Container.Bind<InputFieldGrabber>().AsSingle();
            Container.Bind<MaterialGrabber>().AsSingle();
            Container.Bind<AnimationGrabber>().AsSingle();
        }
    }
}
