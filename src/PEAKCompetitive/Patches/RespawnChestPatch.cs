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

            // Find dead/passed out TEAMMATES only
            List<Character> deadTeammates = new List<Character>();
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
                    }
                }
            }

            // If there are dead teammates, revive them at the chest
            if (deadTeammates.Count > 0 && Ascents.canReviveDead)
            {
                Plugin.Logger.LogInfo($"RespawnChest: Reviving {deadTeammates.Count} teammates");

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

                try
                {
                    // Directly call the Spawner base class SpawnItems method
                    // This bypasses the RespawnChest revival logic and just spawns items
                    var spawnerType = typeof(Spawner);
                    var spawnMethod = spawnerType.GetMethod("SpawnItems",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (spawnMethod != null)
                    {
                        Plugin.Logger.LogInfo($"RespawnChest: Found Spawner.SpawnItems, calling it...");
                        var result = spawnMethod.Invoke(__instance, new object[] { spawnSpots });
                        __result = result as List<PhotonView> ?? new List<PhotonView>();
                        Plugin.Logger.LogInfo($"RespawnChest: Spawner.SpawnItems returned {__result.Count} items");
                    }
                    else
                    {
                        Plugin.Logger.LogError("RespawnChest: Could not find Spawner.SpawnItems method!");
                        __result = new List<PhotonView>();
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.LogError($"RespawnChest: Exception spawning items: {ex.Message}");
                    Plugin.Logger.LogError($"RespawnChest: Stack trace: {ex.StackTrace}");
                    __result = new List<PhotonView>();
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
