using IPA;
using IPA.Config;
using IPA.Config.Stores;
using MorePlaylists.Installers;
using MorePlaylists.Utilities;
using SiraUtil.Zenject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace MorePlaylists
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Instance = this;
            Plugin.Log = logger;
            zenjector.OnMenu<MorePlaylistsMenuInstaller>();
        }

        #region Disableable
        [OnEnable, OnDisable]
        public void OnStateChange()
        {
        }
        #endregion
    }
}
