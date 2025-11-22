using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

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

            // Apply all Harmony patches
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Logger.LogInfo("Competitive features enabled!");
        }

        private void OnDestroy()
        {
            // Cleanup
            _harmony?.UnpatchSelf();
        }
    }
}
