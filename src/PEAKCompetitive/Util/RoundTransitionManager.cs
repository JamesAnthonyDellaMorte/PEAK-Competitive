using UnityEngine;
using Photon.Pun;
using PEAKCompetitive.Model;
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
            // Step 1: Kill all players
            Plugin.Logger.LogInfo("Step 1: Killing all players...");
            KillAllPlayers();

            yield return new WaitForSeconds(2f); // Wait 2 seconds

            // Step 2: Revive all players
            Plugin.Logger.LogInfo("Step 2: Reviving all players...");
            ReviveAllPlayers();

            yield return new WaitForSeconds(1f); // Wait 1 second

            // Step 3: Reset round state and start new round
            Plugin.Logger.LogInfo("Step 3: Starting next round...");
            StartNextRound();
        }

        private void KillAllPlayers()
        {
            // Get all characters
            foreach (var character in Character.AllCharacters)
            {
                if (character == null) continue;

                Plugin.Logger.LogInfo($"Killing character with ViewID: {character.view?.ViewID}");

                // Kill the character using CharacterHelper
                CharacterHelper.KillCharacter(character);
            }

            // Sync kill event to all clients
            NetworkSyncManager.Instance.SyncKillAllPlayers();

            Plugin.Logger.LogInfo($"Killed {Character.AllCharacters.Count} characters");
        }

        private void ReviveAllPlayers()
        {
            // Get next campfire position for teleportation
            Vector3? teleportPos = CharacterHelper.GetNextCampfirePosition();

            // Revive all characters
            foreach (var character in Character.AllCharacters)
            {
                if (character == null) continue;

                Plugin.Logger.LogInfo($"Reviving character with ViewID: {character.view?.ViewID}");

                // Revive the character
                CharacterHelper.ReviveCharacter(character);

                // Teleport to next campfire if position found
                if (teleportPos.HasValue)
                {
                    CharacterHelper.TeleportCharacter(character, teleportPos.Value);
                    Plugin.Logger.LogInfo($"Teleported character to next campfire at {teleportPos.Value}");
                }
            }

            // Sync revive event to all clients
            NetworkSyncManager.Instance.SyncReviveAllPlayers();

            Plugin.Logger.LogInfo($"Revived and teleported {Character.AllCharacters.Count} characters");
        }

        private void StartNextRound()
        {
            var matchState = MatchState.Instance;

            // Reset team round states
            foreach (var team in matchState.Teams)
            {
                team.ResetRoundState();
            }

            // Get next map name (biome progression)
            string nextMap = GetNextBiome(matchState.CurrentMapName);

            // Start new round
            matchState.StartRound(nextMap);

            // Sync round start to all clients
            NetworkSyncManager.Instance.SyncRoundStart(nextMap);

            Plugin.Logger.LogInfo($"New round started: {nextMap}");
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
