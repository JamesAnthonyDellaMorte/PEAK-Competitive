using HarmonyLib;
using Photon.Pun;
using PEAKCompetitive.Model;
using PEAKCompetitive.Util;
using UnityEngine;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Detects when players reach campfires and handles point accumulation
    /// </summary>
    [HarmonyPatch(typeof(Campfire), "Awake")]
    public class CampfireAwakePatch
    {
        static void Postfix(Campfire __instance)
        {
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;

            Plugin.Logger.LogInfo($"Campfire detected at segment: {__instance.advanceToSegment}");

            // Add interaction component to campfire
            var interaction = __instance.gameObject.AddComponent<CampfireInteraction>();
            interaction.campfire = __instance;
        }
    }

    /// <summary>
    /// Handles campfire interaction logic
    /// </summary>
    public class CampfireInteraction : MonoBehaviour
    {
        public Campfire campfire;
        private const float INTERACTION_RADIUS = 5f; // 5 meters from campfire

        private void Update()
        {
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;
            if (!MatchState.Instance.IsRoundActive) return;

            // Check if local player is near this campfire
            CheckPlayerProximity();
        }

        private void CheckPlayerProximity()
        {
            // Get local character
            var localPlayer = PhotonNetwork.LocalPlayer;
            if (localPlayer == null) return;

            // TODO: Get local character's position from PEAK's Character system
            // For now, we'll need to hook into Character detection
            // Character localCharacter = GetLocalCharacter();
            // if (localCharacter == null) return;

            // float distance = Vector3.Distance(localCharacter.transform.position, campfire.transform.position);

            // For placeholder, we'll detect based on trigger colliders in OnTriggerStay
        }

        private void OnTriggerStay(Collider other)
        {
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;
            if (!MatchState.Instance.IsRoundActive) return;

            // Check if this is a player character
            var character = other.GetComponent<Character>();
            if (character == null) return;

            // Check if this is the local player
            if (!character.view.IsMine) return;

            // Get player's team
            var localPlayer = PhotonNetwork.LocalPlayer;
            var team = TeamManager.GetPlayerTeam(localPlayer);

            if (team == null)
            {
                Plugin.Logger.LogWarning("Local player not assigned to a team!");
                return;
            }

            // Check if team has reached summit
            if (!team.HasReachedSummit)
            {
                // First time reaching summit for this team
                OnTeamReachedSummit(team, character);
            }
        }

        private void OnTeamReachedSummit(TeamData team, Character character)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                // Non-host: Send message to host
                // TODO: Use RPC to notify host
                Plugin.Logger.LogInfo($"Player reached campfire, notifying host...");
                return;
            }

            Plugin.Logger.LogInfo($"{team.TeamName} reached the campfire!");

            // Mark team as reached summit
            team.HasReachedSummit = true;

            // Check if this is the first team to finish
            bool isFirstTeam = !RoundTimerManager.Instance.IsTimerActive;

            if (isFirstTeam)
            {
                Plugin.Logger.LogInfo($"{team.TeamName} is the FIRST team to reach the campfire! Starting 10-minute timer...");
                RoundTimerManager.Instance.StartTimer();
            }

            // Count living team members
            int livingMembers = team.GetAlivePlayersCount();

            Plugin.Logger.LogInfo($"{team.TeamName} has {livingMembers}/{team.Members.Count} members alive");

            // Calculate and award points
            int baseMapPoints = Configuration.ConfigurationHandler.GetMapPoints(MatchState.Instance.CurrentMapName);
            float totalPoints = ScoringCalculator.CalculateRoundPoints(team, baseMapPoints, livingMembers);

            team.AddScore((int)totalPoints);

            Plugin.Logger.LogInfo($"{team.TeamName} earned {totalPoints} points! New total: {team.Score}");

            // Sync to all clients
            NetworkSyncManager.Instance.SyncTeamAssignments(); // Includes scores

            // Check if all teams have finished
            if (AllTeamsFinished())
            {
                Plugin.Logger.LogInfo("All teams finished! Ending round early...");
                RoundTimerManager.Instance.StopTimer();
                RoundTransitionManager.Instance.StartTransition();
            }
        }

        private bool AllTeamsFinished()
        {
            var matchState = MatchState.Instance;

            foreach (var team in matchState.Teams)
            {
                if (!team.HasReachedSummit)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
