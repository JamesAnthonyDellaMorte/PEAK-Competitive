using UnityEngine;
using Photon.Pun;
using PEAKCompetitive.Model;
using PEAKCompetitive.Patches;
using System.Collections;

namespace PEAKCompetitive.Util
{
    public class RoundTransitionManager : MonoBehaviour
    {
        private static RoundTransitionManager _instance;

        public static RoundTransitionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("RoundTransitionManager");
                    _instance = go.AddComponent<RoundTransitionManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void StartTransition()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo("Starting round transition: Kill -> Revive -> Next Round");

            StartCoroutine(TransitionSequence());
        }

        private IEnumerator TransitionSequence()
        {
            // Mark round as inactive to prevent campfire detection during transition
            MatchState.Instance.IsRoundActive = false;
            Plugin.Logger.LogInfo("Round marked inactive during transition");

            // Step 1: Kill all players
            Plugin.Logger.LogInfo("Step 1: Killing all players...");
            KillAllPlayers();

            yield return new WaitForSeconds(2f); // Wait 2 seconds

            // Step 2: Get next campfire position BEFORE reviving
            Vector3? teleportPos = CharacterHelper.GetNextCampfirePosition();
            Plugin.Logger.LogInfo($"Next campfire position: {(teleportPos.HasValue ? teleportPos.Value.ToString() : "NOT FOUND")}");

            // Step 3: Revive all players at campfire position
            Plugin.Logger.LogInfo("Step 2: Reviving all players at campfire...");
            ReviveAllPlayersAtPosition(teleportPos);

            yield return new WaitForSeconds(1f); // Wait 1 second

            // Step 4: Reset round state and start new round
            Plugin.Logger.LogInfo("Step 3: Starting next round...");
            StartNextRound();
        }

        private void KillAllPlayers()
        {
            // Send RPC to all clients to kill their own character
            NetworkSyncManager.Instance.SyncKillAllPlayers();
            Plugin.Logger.LogInfo("Sent kill RPC to all clients");
        }

        private void ReviveAllPlayersAtPosition(Vector3? teleportPosition)
        {
            // Send RPC to all clients to revive and teleport their own character
            NetworkSyncManager.Instance.SyncReviveAllPlayers(teleportPosition);
            Plugin.Logger.LogInfo("Sent revive & teleport RPC to all clients");
        }

        private void StartNextRound()
        {
            var matchState = MatchState.Instance;

            // Reset team round states
            foreach (var team in matchState.Teams)
            {
                team.ResetRoundState();
            }

            // Reset RespawnChest tracking for the new round
            RespawnChestPatch.ResetChestTracking();

            // Reset campfire detection for all campfires
            CampfireInteraction.ResetAllDetections();

            // Get next map name (biome progression)
            string nextMap = GetNextBiome(matchState.CurrentMapName);

            // Set the target campfire for detection
            Segment nextSegment = GetNextSegment(matchState.CurrentMapName);
            CampfireInteraction.SetRoundTarget(nextSegment);
            Plugin.Logger.LogInfo($"Set round target segment: {nextSegment}");

            // Start new round
            matchState.StartRound(nextMap);

            // Sync round start to all clients
            NetworkSyncManager.Instance.SyncRoundStart(nextMap);

            Plugin.Logger.LogInfo($"New round started: {nextMap}");
        }

        private Segment GetNextSegment(string currentBiome)
        {
            currentBiome = currentBiome?.ToLower() ?? "";

            if (currentBiome.Contains("shore") || currentBiome.Contains("beach") || currentBiome == "")
                return Segment.Tropics;
            if (currentBiome.Contains("tropic"))
                return Segment.Alpine;
            if (currentBiome.Contains("mesa") || currentBiome.Contains("alpine"))
                return Segment.Caldera;
            if (currentBiome.Contains("root") || currentBiome.Contains("caldera"))
                return Segment.TheKiln;
            if (currentBiome.Contains("kiln"))
                return Segment.Peak;

            return Segment.Tropics; // Default
        }

        private string GetNextBiome(string currentBiome)
        {
            // PEAK biome progression order:
            // Shore → Tropics → Mesa → Alpine → Roots → Caldera → Kiln
            string[] biomeOrder = { "Shore", "Tropics", "Mesa", "Alpine", "Roots", "Caldera", "Kiln" };

            for (int i = 0; i < biomeOrder.Length - 1; i++)
            {
                if (currentBiome.Contains(biomeOrder[i]))
                {
                    return biomeOrder[i + 1];
                }
            }

            // If at Kiln or unknown, end match
            if (currentBiome.Contains("Kiln"))
            {
                Plugin.Logger.LogInfo("Reached final biome (Kiln). Ending match!");
                MatchState.Instance.EndMatch();
                return "Kiln";
            }

            // Default to Shore if unknown
            return "Shore";
        }
    }
}
