using HarmonyLib;
using Photon.Pun;
using PEAKCompetitive.Model;
using PEAKCompetitive.Util;
using UnityEngine;
using System.Collections.Generic;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Detects when campfires are LIT (not just approached).
    /// This hooks into the actual campfire lighting event, which only fires
    /// when the vanilla game confirms all players are in range.
    /// </summary>
    [HarmonyPatch(typeof(Campfire), "Light_Rpc")]
    public class CampfireLightPatch
    {
        // Track which campfires have been lit this round (to prevent double-triggering)
        private static HashSet<int> _litCampfiresThisRound = new HashSet<int>();

        // Track the target segment for this round
        private static Segment _targetSegment = Segment.Tropics;

        /// <summary>
        /// Set the target segment for this round
        /// </summary>
        public static void SetTargetSegment(Segment segment)
        {
            _targetSegment = segment;
            Plugin.Logger.LogInfo($"[Campfire] Target segment set to: {segment}");
        }

        /// <summary>
        /// Reset for new round
        /// </summary>
        public static void ResetForNewRound()
        {
            _litCampfiresThisRound.Clear();
            Plugin.Logger.LogInfo("[Campfire] Reset lit campfires tracking for new round");
        }

        /// <summary>
        /// Called when ANY campfire is lit via RPC
        /// </summary>
        static void Postfix(Campfire __instance)
        {
            // Only process in competitive mode with active match
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;
            if (!MatchState.Instance.IsMatchActive) return;
            if (!MatchState.Instance.IsRoundActive) return;

            // Only master client processes scoring/transitions
            if (!PhotonNetwork.IsMasterClient) return;

            int campfireId = __instance.GetInstanceID();

            // Check if we already processed this campfire this round
            if (_litCampfiresThisRound.Contains(campfireId))
            {
                Plugin.Logger.LogInfo($"[Campfire] Already processed campfire {campfireId} this round");
                return;
            }

            // Check if this campfire advances to the target segment
            if (__instance.advanceToSegment != _targetSegment)
            {
                Plugin.Logger.LogInfo($"[Campfire] Lit campfire advances to {__instance.advanceToSegment}, but target is {_targetSegment} - ignoring");
                return;
            }

            // This is the target campfire and it was just lit!
            _litCampfiresThisRound.Add(campfireId);

            Plugin.Logger.LogInfo($"");
            Plugin.Logger.LogInfo($"========================================");
            Plugin.Logger.LogInfo($"=== TARGET CAMPFIRE LIT! ===");
            Plugin.Logger.LogInfo($"========================================");
            Plugin.Logger.LogInfo($"Campfire advances to: {__instance.advanceToSegment}");
            Plugin.Logger.LogInfo($"Position: {__instance.transform.position}");

            // Award points to all players who are nearby (within vanilla's 15m range)
            ProcessCampfireCompletion(__instance);
        }

        /// <summary>
        /// Process the campfire completion - award points and trigger round transition
        /// </summary>
        private static void ProcessCampfireCompletion(Campfire campfire)
        {
            Vector3 campfirePos = campfire.transform.position;
            float detectionRange = 20f; // Slightly larger than vanilla's 15m to be safe

            // Find all players near the campfire and award points by team
            Dictionary<int, List<Photon.Realtime.Player>> teamPlayers = new Dictionary<int, List<Photon.Realtime.Player>>();
            Dictionary<int, int> teamNonGhostCount = new Dictionary<int, int>();

            foreach (var character in Character.AllCharacters)
            {
                if (character == null || character.view == null) continue;

                var owner = character.view.Owner;
                if (owner == null) continue;

                // Check if player is near campfire
                float distance = Vector3.Distance(character.Center, campfirePos);
                if (distance > detectionRange) continue;

                // Get player's team
                var team = TeamManager.GetPlayerTeam(owner);
                if (team == null) continue;

                // Track player by team
                if (!teamPlayers.ContainsKey(team.TeamId))
                {
                    teamPlayers[team.TeamId] = new List<Photon.Realtime.Player>();
                    teamNonGhostCount[team.TeamId] = 0;
                }

                teamPlayers[team.TeamId].Add(owner);

                // Count non-ghosts
                if (!character.IsGhost)
                {
                    teamNonGhostCount[team.TeamId]++;
                }

                Plugin.Logger.LogInfo($"[Campfire] Player {TeamManager.GetPlayerDisplayName(owner)} from {team.TeamName} at campfire (ghost: {character.IsGhost})");
            }

            // Award points to each team based on their non-ghost members
            int placement = 1;
            foreach (var teamId in teamPlayers.Keys)
            {
                var team = MatchState.Instance.Teams.Find(t => t.TeamId == teamId);
                if (team == null) continue;

                int nonGhostCount = teamNonGhostCount[teamId];
                if (nonGhostCount == 0)
                {
                    Plugin.Logger.LogInfo($"[Campfire] {team.TeamName} has no living members - no points");
                    continue;
                }

                // Calculate points: placement multiplier × base points × living members
                int basePoints = Configuration.ConfigurationHandler.GetMapPoints(MatchState.Instance.CurrentMapName);
                int multiplier = GetPlacementMultiplier(placement);
                int points = multiplier * basePoints * nonGhostCount;

                team.AddScore(points);
                team.HasReachedSummit = true;

                Plugin.Logger.LogInfo($"=== POINTS AWARDED ===");
                Plugin.Logger.LogInfo($"{team.TeamName}: {multiplier}x × {basePoints} base × {nonGhostCount} alive = {points} points");
                Plugin.Logger.LogInfo($"{team.TeamName} new total: {team.Score}");

                placement++;
            }

            // Sync scores to all clients
            NetworkSyncManager.Instance.SyncTeamScores();

            // Start the round timer (gives other teams time to catch up if they weren't all there)
            if (!RoundTimerManager.Instance.IsTimerActive)
            {
                Plugin.Logger.LogInfo("[Campfire] Starting 10-minute round timer");
                RoundTimerManager.Instance.StartTimer();
                NetworkSyncManager.Instance.SyncTimerStart();
            }

            // Check if all teams have finished
            if (AllTeamsFinished())
            {
                Plugin.Logger.LogInfo("[Campfire] All teams finished! Starting round transition...");
                RoundTimerManager.Instance.StopTimer();
                RoundTransitionManager.Instance.StartTransition();
            }
        }

        private static int GetPlacementMultiplier(int placement)
        {
            switch (placement)
            {
                case 1: return 4;
                case 2: return 3;
                case 3: return 2;
                default: return 1;
            }
        }

        private static bool AllTeamsFinished()
        {
            int teamsWithPlayers = 0;
            int teamsFinished = 0;

            foreach (var team in MatchState.Instance.Teams)
            {
                if (team.Members.Count == 0) continue;
                teamsWithPlayers++;
                if (team.HasReachedSummit)
                    teamsFinished++;
            }

            return teamsWithPlayers > 0 && teamsFinished == teamsWithPlayers;
        }
    }

    /// <summary>
    /// Legacy patch for campfire awake - just logs campfire detection
    /// </summary>
    [HarmonyPatch(typeof(Campfire), "Awake")]
    public class CampfireAwakePatch
    {
        static void Postfix(Campfire __instance)
        {
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;
            Plugin.Logger.LogInfo($"Campfire detected! AdvancesTo: {__instance.advanceToSegment}, Position: {__instance.transform.position}");
        }
    }

    /// <summary>
    /// Static helper class for campfire management
    /// </summary>
    public static class CampfireInteraction
    {
        /// <summary>
        /// Set the target segment for this round (called by RoundTransitionManager)
        /// </summary>
        public static void SetRoundTarget(Segment targetSegment)
        {
            CampfireLightPatch.SetTargetSegment(targetSegment);
        }

        /// <summary>
        /// Reset all detections for new round
        /// </summary>
        public static void ResetAllDetections()
        {
            CampfireLightPatch.ResetForNewRound();
        }
    }
}
