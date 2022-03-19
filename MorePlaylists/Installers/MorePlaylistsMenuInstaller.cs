﻿using MorePlaylists.BeatSaver;
using MorePlaylists.UI;
using MorePlaylists.Utilities;
using Zenject;

namespace MorePlaylists.Installers
{
    public class MorePlaylistsMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<BeatSaver.BeatSaver>().AsSingle();
            Container.BindInterfacesTo<Hitbloq.Hitbloq>().AsSingle();
            Container.BindInterfacesTo<AccSaber.AccSaber>().AsSingle();

            Container.Bind<BasicListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BeatSaverListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MorePlaylistsSongListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MorePlaylistsDownloaderViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BasicDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<MorePlaylistsNavigationController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesAndSelfTo<PopupModalsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SourceModalController>().AsSingle();

            Container.BindInterfacesAndSelfTo<MorePlaylistsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonUI>().AsSingle();

            Container.Bind<SpriteLoader>().AsSingle();
            Container.Bind<InputFieldGrabber>().AsSingle();
        }
    }
}
