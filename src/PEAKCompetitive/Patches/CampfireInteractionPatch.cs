using HarmonyLib;
using Photon.Pun;
using PEAKCompetitive.Model;
using PEAKCompetitive.Util;
using UnityEngine;
using System.Collections.Generic;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Detects when players reach campfires and handles point accumulation.
    /// Uses distance-based detection instead of trigger colliders.
    /// Only detects campfires that advance to the NEXT segment (not current).
    /// </summary>
    [HarmonyPatch(typeof(Campfire), "Awake")]
    public class CampfireAwakePatch
    {
        static void Postfix(Campfire __instance)
        {
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;

            Plugin.Logger.LogInfo($"Campfire detected! AdvancesTo: {__instance.advanceToSegment}, Position: {__instance.transform.position}");

            // Add interaction component to campfire
            var interaction = __instance.gameObject.AddComponent<CampfireInteraction>();
            interaction.campfire = __instance;
        }
    }

    /// <summary>
    /// Handles campfire interaction logic using distance-based detection
    /// </summary>
    public class CampfireInteraction : MonoBehaviour
    {
        public Campfire campfire;

        // Detection radius - how close player needs to be to trigger arrival
        private const float DETECTION_RADIUS = 10f;

        // Grace period after round starts before detection begins (seconds)
        private const float GRACE_PERIOD = 5f;

        // Track which players have already been detected at this campfire (to avoid spam)
        private HashSet<int> _detectedPlayers = new HashSet<int>();

        // Cooldown to prevent rapid re-detection
        private float _detectionCooldown = 0f;

        // Time when this campfire became valid for detection
        private float _validDetectionTime = 0f;

        // Track if we've logged the target campfire info
        private bool _loggedTargetInfo = false;

        private void Start()
        {
            // Set initial grace period
            _validDetectionTime = Time.time + GRACE_PERIOD;
        }

        private void Update()
        {
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;
            if (!MatchState.Instance.IsMatchActive) return;

            // Update cooldown
            if (_detectionCooldown > 0f)
            {
                _detectionCooldown -= Time.deltaTime;
                return;
            }

            // Check grace period
            if (Time.time < _validDetectionTime) return;

            // Check if this campfire is the target (advances to next segment)
            if (!IsTargetCampfire())
            {
                return;
            }

            // Check if local player is near this campfire
            CheckLocalPlayerProximity();
        }

        /// <summary>
        /// Check if this campfire is the one players should be heading to.
        /// Only campfires that advance BEYOND the current segment are valid targets.
        /// </summary>
        private bool IsTargetCampfire()
        {
            // Get the current segment from the map name
            Segment currentSegment = GetCurrentSegment();
            Segment campfireAdvancesTo = campfire.advanceToSegment;

            // Log target info once per campfire
            if (!_loggedTargetInfo && MatchState.Instance.IsMatchActive)
            {
                Plugin.Logger.LogInfo($"[Campfire] Current segment: {currentSegment}, This campfire advances to: {campfireAdvancesTo}");
                _loggedTargetInfo = true;
            }

            // The campfire is a valid target if it advances to a segment AFTER the current one
            // Segment enum: Beach=0, Tropics=1, Alpine=2, Caldera=3, TheKiln=4, Peak=5
            return (int)campfireAdvancesTo > (int)currentSegment;
        }

        /// <summary>
        /// Get the current segment based on the match state's current map name
        /// </summary>
        private Segment GetCurrentSegment()
        {
            string mapName = MatchState.Instance.CurrentMapName?.ToLower() ?? "";

            // Map names to segments
            if (mapName.Contains("shore") || mapName.Contains("beach"))
                return Segment.Beach;
            if (mapName.Contains("tropic"))
                return Segment.Tropics;
            if (mapName.Contains("mesa") || mapName.Contains("alpine"))
                return Segment.Alpine;
            if (mapName.Contains("root") || mapName.Contains("caldera"))
                return Segment.Caldera;
            if (mapName.Contains("kiln"))
                return Segment.TheKiln;
            if (mapName.Contains("peak") || mapName.Contains("summit"))
                return Segment.Peak;

            // Default to Beach (start)
            return Segment.Beach;
        }

        private void CheckLocalPlayerProximity()
        {
            // Get local character using CharacterHelper
            var localCharacter = CharacterHelper.GetLocalCharacter();
            if (localCharacter == null) return;

            // Get local player
            var localPlayer = PhotonNetwork.LocalPlayer;
            if (localPlayer == null) return;

            // Skip if already detected at this campfire
            if (_detectedPlayers.Contains(localPlayer.ActorNumber)) return;

            // Calculate distance to campfire
            Vector3 playerPos = localCharacter.Center;
            Vector3 campfirePos = campfire.transform.position;
            float distance = Vector3.Distance(playerPos, campfirePos);

            // Debug log occasionally (every 5 seconds)
            if (Time.frameCount % 300 == 0)
            {
                Plugin.Logger.LogInfo($"[Campfire Check] Distance: {distance:F1}m to {campfire.advanceToSegment} campfire, Radius: {DETECTION_RADIUS}m");
            }

            // Check if within detection radius
            if (distance <= DETECTION_RADIUS)
            {
                OnPlayerReachedCampfire(localPlayer, localCharacter);
            }
        }

        private void OnPlayerReachedCampfire(Photon.Realtime.Player player, Character character)
        {
            // Mark as detected to avoid spam
            _detectedPlayers.Add(player.ActorNumber);
            _detectionCooldown = 2f; // 2 second cooldown

            // Check if player is a ghost
            bool isGhost = character.IsGhost;

            Plugin.Logger.LogInfo($"");
            Plugin.Logger.LogInfo($"========================================");
            Plugin.Logger.LogInfo($"=== CAMPFIRE ARRIVAL DETECTED ===");
            Plugin.Logger.LogInfo($"========================================");
            Plugin.Logger.LogInfo($"Player: {TeamManager.GetPlayerDisplayName(player)} (Actor {player.ActorNumber})");
            Plugin.Logger.LogInfo($"Campfire advances to: {campfire.advanceToSegment}");
            Plugin.Logger.LogInfo($"Is Ghost: {isGhost}");

            // Get player's team
            var team = TeamManager.GetPlayerTeam(player);
            if (team == null)
            {
                Plugin.Logger.LogWarning($"Player {player.ActorNumber} not assigned to any team!");
                return;
            }

            Plugin.Logger.LogInfo($"Team: {team.TeamName} (ID: {team.TeamId})");

            // Notify the host about this arrival
            Plugin.Logger.LogInfo($"Sending arrival notification to host...");
            NetworkSyncManager.Instance.NotifyHostOfCampfireArrival(player.ActorNumber, team.TeamId, isGhost);
        }

        /// <summary>
        /// Reset detection state (call when starting a new round)
        /// </summary>
        public void ResetDetection()
        {
            _detectedPlayers.Clear();
            _detectionCooldown = 0f;
            _validDetectionTime = Time.time + GRACE_PERIOD;
            _loggedTargetInfo = false;
            Plugin.Logger.LogInfo($"Campfire detection reset for {campfire.advanceToSegment} campfire (grace period: {GRACE_PERIOD}s)");
        }

        /// <summary>
        /// Static method to reset all campfire detections
        /// </summary>
        public static void ResetAllDetections()
        {
            var interactions = FindObjectsByType<CampfireInteraction>(FindObjectsSortMode.None);
            foreach (var interaction in interactions)
            {
                interaction.ResetDetection();
            }
            Plugin.Logger.LogInfo($"Reset detection for {interactions.Length} campfires");
        }
    }
}
