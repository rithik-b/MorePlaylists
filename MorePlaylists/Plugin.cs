using IPA;
using MorePlaylists.Installers;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace MorePlaylists
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin? Instance { get; private set; }
        internal static IPALogger? Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            zenjector.UseHttpService();
            zenjector.UseMetadataBinder<Plugin>();
            zenjector.Install<MorePlaylistsMenuInstaller>(Location.Menu);
        }

        #region Disableable
        [OnEnable, OnDisable]
        public void OnStateChange()
        {
        }
        #endregion
    }
}
