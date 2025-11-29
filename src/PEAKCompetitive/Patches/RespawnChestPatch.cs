using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using PEAKCompetitive.Configuration;
using PEAKCompetitive.Model;
using PEAKCompetitive.Util;
using UnityEngine;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Patches RespawnChest (checkpoint statue) to:
    /// 1. Only revive TEAMMATES (same team as the interactor)
    /// 2. If no dead teammates, spawn legendary item for the interactor
    /// </summary>
    [HarmonyPatch(typeof(RespawnChest), "SpawnItems")]
    public class RespawnChestPatch
    {
        // Track which chest has been used (to give legendary to first team only)
        private static HashSet<int> _usedChestIds = new HashSet<int>();

        // Flag to prevent re-entry when we want to run original logic
        private static bool _allowOriginal = false;

        /// <summary>
        /// Reset tracking when a new round starts
        /// </summary>
        public static void ResetChestTracking()
        {
            _usedChestIds.Clear();
            Plugin.Logger.LogInfo("RespawnChest tracking reset for new round");
        }

        /// <summary>
        /// Prefix that handles team-based revival and legendary item spawning.
        /// </summary>
        static bool Prefix(RespawnChest __instance, List<Transform> spawnSpots, ref List<PhotonView> __result)
        {
            // Check for re-entry (we set this flag when we want original to run)
            if (_allowOriginal)
            {
                Plugin.Logger.LogInfo("RespawnChest: Re-entry detected, letting original run for item spawn");
                return true;
            }

            Plugin.Logger.LogInfo($"=== RESPAWN CHEST PATCH TRIGGERED ===");
            Plugin.Logger.LogInfo($"CompetitiveMode: {ConfigurationHandler.EnableCompetitiveMode}");
            Plugin.Logger.LogInfo($"MatchActive: {MatchState.Instance.IsMatchActive}");
            Plugin.Logger.LogInfo($"IsMasterClient: {PhotonNetwork.IsMasterClient}");
            Plugin.Logger.LogInfo($"SpawnSpots count: {spawnSpots?.Count ?? 0}");

            // If competitive mode is disabled, let original method run
            if (!ConfigurationHandler.EnableCompetitiveMode)
            {
                Plugin.Logger.LogInfo("Competitive mode disabled - using default behavior");
                return true;
            }

            // If no match is active, let original method run
            if (!MatchState.Instance.IsMatchActive)
            {
                Plugin.Logger.LogInfo("Match not active - using default behavior");
                return true;
            }

            // Only master client handles this
            if (!PhotonNetwork.IsMasterClient)
            {
                Plugin.Logger.LogInfo("Not master client - skipping");
                __result = new List<PhotonView>();
                return false;
            }

            // Get the chest's instance ID for tracking
            int chestId = __instance.GetInstanceID();

            // Find which character interacted with this chest
            // We need to find the closest living character to the chest
            Character interactor = FindClosestLivingCharacter(__instance.transform.position);

            if (interactor == null || interactor.view?.Owner == null)
            {
                Plugin.Logger.LogWarning("RespawnChest: Could not determine interactor");
                __result = new List<PhotonView>();
                return false;
            }

            // Get the interactor's team
            var interactorTeam = TeamManager.GetPlayerTeam(interactor.view.Owner);
            if (interactorTeam == null)
            {
                Plugin.Logger.LogWarning("RespawnChest: Interactor not on a team - using default behavior");
                return true;
            }

            Plugin.Logger.LogInfo($"RespawnChest: {TeamManager.GetPlayerDisplayName(interactor.view.Owner)} from {interactorTeam.TeamName} interacted");

            // Find dead/passed out characters categorized by team
            List<Character> deadTeammates = new List<Character>();
            List<Character> deadEnemies = new List<Character>();

            foreach (Character character in Character.AllCharacters)
            {
                if (character.data.dead || character.data.fullyPassedOut)
                {
                    var charOwner = character.view?.Owner;
                    if (charOwner != null)
                    {
                        var charTeam = TeamManager.GetPlayerTeam(charOwner);
                        if (charTeam != null && charTeam.TeamId == interactorTeam.TeamId)
                        {
                            deadTeammates.Add(character);
                        }
                        else
                        {
                            deadEnemies.Add(character);
                        }
                    }
                }
            }

            Plugin.Logger.LogInfo($"RespawnChest: Dead teammates: {deadTeammates.Count}, Dead enemies: {deadEnemies.Count}");

            // If there are dead teammates, revive them at the chest
            if (deadTeammates.Count > 0 && Ascents.canReviveDead)
            {
                Plugin.Logger.LogInfo($"RespawnChest: Reviving {deadTeammates.Count} teammates");

                // Remove the skeleton visual
                __instance.photonView.RPC("RemoveSkeletonRPC", RpcTarget.AllBuffered, System.Array.Empty<object>());

                // Revive ALL dead teammates (same behavior as original but team-filtered)
                foreach (var teammate in deadTeammates)
                {
                    teammate.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, new object[]
                    {
                        __instance.transform.position + Vector3.up * 8f,
                        true // poof effect
                    });
                    Plugin.Logger.LogInfo($"  Revived: {TeamManager.GetPlayerDisplayName(teammate.view.Owner)}");
                }

                __result = new List<PhotonView>();
                return false;
            }

            // No dead teammates - check if this team gets the legendary item
            // First team to touch (when chest hasn't been used) gets the item
            if (!_usedChestIds.Contains(chestId))
            {
                _usedChestIds.Add(chestId);
                Plugin.Logger.LogInfo($"RespawnChest: {interactorTeam.TeamName} is first to touch - spawning legendary item!");

                // Check if any enemies are dead (would trigger vanilla revival logic)
                if (deadEnemies.Count > 0)
                {
                    Plugin.Logger.LogInfo($"RespawnChest: Enemies are dead but not teammates - forcing item spawn mode");
                    // We need to spawn items WITHOUT reviving enemies
                    // Use re-entry flag so original runs but we handle it in Postfix
                    // Actually, we can just call the base Spawner.SpawnItems directly

                    try
                    {
                        // Get the spawn spots from the chest
                        var getSpotsMethod = typeof(Spawner).GetMethod("GetSpawnSpots",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        List<Transform> spots = spawnSpots;
                        if (getSpotsMethod != null)
                        {
                            spots = getSpotsMethod.Invoke(__instance, null) as List<Transform> ?? spawnSpots;
                        }

                        // Use the _allowOriginal flag to bypass our Prefix on re-entry
                        _allowOriginal = true;
                        try
                        {
                            // Call the original method - our Prefix will see _allowOriginal and let it through
                            // But we need to make the original think there are no dead players
                            // Since we can't modify that check, we'll call Spawner.SpawnItems directly

                            // Actually, let's just call ForceSpawn or TrySpawnItems
                            var forceSpawnMethod = typeof(Spawner).GetMethod("ForceSpawn",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            if (forceSpawnMethod != null)
                            {
                                Plugin.Logger.LogInfo("RespawnChest: Calling Spawner.ForceSpawn()");
                                forceSpawnMethod.Invoke(__instance, null);
                                __result = new List<PhotonView>();
                            }
                            else
                            {
                                Plugin.Logger.LogError("RespawnChest: Could not find ForceSpawn method");
                                __result = new List<PhotonView>();
                            }
                        }
                        finally
                        {
                            _allowOriginal = false;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Plugin.Logger.LogError($"RespawnChest: Exception spawning items: {ex.Message}");
                        Plugin.Logger.LogError($"RespawnChest: Stack trace: {ex.StackTrace}");
                        __result = new List<PhotonView>();
                        _allowOriginal = false;
                    }
                }
                else
                {
                    Plugin.Logger.LogInfo("RespawnChest: No dead players at all - letting original spawn items");
                    // No one is dead - original will naturally spawn items (not try to revive)
                    // Character.PlayerIsDeadOrDown() will return false
                    return true;
                }
            }
            else
            {
                Plugin.Logger.LogInfo($"RespawnChest: Already used by another team - no item for {interactorTeam.TeamName}");
                __result = new List<PhotonView>();
            }

            return false;
        }

        /// <summary>
        /// Find the closest living character to a position (the one who interacted)
        /// </summary>
        private static Character FindClosestLivingCharacter(Vector3 position)
        {
            Character closest = null;
            float closestDist = float.MaxValue;

            foreach (var character in Character.AllCharacters)
            {
                if (character == null) continue;
                if (character.data.dead || character.data.fullyPassedOut) continue;

                float dist = Vector3.Distance(character.Center, position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = character;
                }
            }

            return closest;
        }
    }
}
