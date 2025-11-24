using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace PEAKCompetitive
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        private Harmony _harmony;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loading...");

            // Initialize configuration
            Configuration.ConfigurationHandler.Initialize(Config);

            // Add UI components
            gameObject.AddComponent<Configuration.ScoreboardUI>();
            gameObject.AddComponent<Configuration.CompetitiveMenuUI>();
            gameObject.AddComponent<Configuration.MatchNotificationUI>();
            gameObject.AddComponent<Configuration.RoundTimerUI>();

            // Initialize managers
            _ = Util.NetworkSyncManager.Instance;
            _ = Util.RoundTimerManager.Instance;
            _ = Util.RoundTransitionManager.Instance;

            // Apply all Harmony patches
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Logger.LogInfo("Competitive features enabled!");
            Logger.LogInfo("Press F3 to open competitive menu");
        }

        private void OnDestroy()
        {
            // Cleanup
            _harmony?.UnpatchSelf();
        }
    }
}
