using IPA;
using IPA.Logging;
using MorePlaylists.Installers;
using MorePlaylists.Utilities;
using SiraUtil.Zenject;

namespace MorePlaylists
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
       internal static Logger Log { get; private set; }

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        [Init]
        public Plugin(Logger logger, Zenjector zenjector)
        {
            Log = logger;
            Log.Debug("MorePlaylists initialized.");
            zenjector.OnMenu<MorePlaylistsMenuInstaller>();
            DownloaderUtils.Init();
        }
        
        /// <summary>
        /// Called when the plugin state changes (Plugin getting enabled/disabled or the game starts or exits).
        /// </summary>
        [OnEnable, OnDisable]
        public void OnStateChanged()
        {
            // NOP, SiraUtil/Zenject is poggies
        }
    }
}
