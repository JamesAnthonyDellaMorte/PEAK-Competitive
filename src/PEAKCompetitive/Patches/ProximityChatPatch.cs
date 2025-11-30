using HarmonyLib;
using PEAKCompetitive.Configuration;
using PEAKCompetitive.Model;
using UnityEngine;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Disables proximity chat in competitive FFA mode so everyone can hear each other.
    /// Works by forcing spatialBlend to 0 (global audio) instead of 1 (3D spatial/proximity).
    /// </summary>
    [HarmonyPatch(typeof(CharacterVoiceHandler), "LateUpdate")]
    public class ProximityChatPatch
    {
        /// <summary>
        /// After the original LateUpdate, override spatialBlend if in competitive FFA mode.
        /// Only applies when Free-for-All is enabled - team mode keeps proximity chat!
        /// </summary>
        static void Postfix(CharacterVoiceHandler __instance)
        {
            // Only apply when competitive mode is enabled
            if (!ConfigurationHandler.EnableCompetitiveMode) return;

            // CRITICAL: Only apply in Free-for-All mode!
            // Team mode MUST keep proximity chat so you only hear nearby teammates/enemies
            if (!ConfigurationHandler.FreeForAllMode) return;

            // Check if global voice is enabled for FFA
            if (!ConfigurationHandler.DisableProximityChat) return;

            // Get the audio source (it's an internal property)
            var audioSource = __instance.audioSource;
            if (audioSource != null)
            {
                // Force global audio (no proximity) - everyone hears everyone in FFA
                audioSource.spatialBlend = 0f;
            }
        }
    }
}
