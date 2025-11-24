using HarmonyLib;
using PEAKCompetitive.Configuration;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Disables Scoutmaster Myres spawning during competitive matches.
    /// In competitive mode, teams race independently and will naturally be far apart,
    /// which would trigger unwanted Scoutmaster spawns.
    /// </summary>
    [HarmonyPatch(typeof(ScoutmasterSpawner), "SpawnScoutmaster")]
    public class ScoutmasterSpawnPatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            // Block Scoutmaster spawning when competitive mode is enabled
            if (ConfigurationHandler.EnableCompetitiveMode)
            {
                Plugin.Logger.LogDebug("Blocked Scoutmaster spawn - competitive mode active");
                return false; // Skip original method
            }

            return true; // Allow normal spawning
        }
    }

    /// <summary>
    /// Disables the Call Scoutmaster item/action during competitive matches
    /// </summary>
    [HarmonyPatch(typeof(Action_CallScoutmaster), "Use")]
    public class ScoutmasterCallPatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            // Block manual Scoutmaster calling when competitive mode is enabled
            if (ConfigurationHandler.EnableCompetitiveMode)
            {
                Plugin.Logger.LogInfo("Blocked manual Scoutmaster call - competitive mode active");
                return false; // Skip original method
            }

            return true; // Allow normal use
        }
    }
}
