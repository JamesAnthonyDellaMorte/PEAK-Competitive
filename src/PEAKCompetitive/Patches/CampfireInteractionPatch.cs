using HarmonyLib;
using Photon.Pun;
using PEAKCompetitive.Model;
using PEAKCompetitive.Util;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Detects when players reach campfires and handles point accumulation.
    /// Uses distance-based detection instead of trigger colliders.
    /// Only detects the NEXT campfire in progression, not intermediate ones.
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
        private const float GRACE_PERIOD = 8f;

        // Track which players have already been detected at this campfire (to avoid spam)
        private HashSet<int> _detectedPlayers = new HashSet<int>();

        // Cooldown to prevent rapid re-detection
        private float _detectionCooldown = 0f;

        // Time when this campfire became valid for detection
        private float _validDetectionTime = 0f;

        // Track if we've logged the target campfire info
        private bool _loggedTargetInfo = false;

        // Static: Track which campfire instance is the current round's target
        private static int _currentRoundTargetCampfireId = -1;
        private static Segment _currentRoundTargetSegment = Segment.Beach;

        /// <summary>
        /// Set the target campfire for this round. Called when round starts.
        /// </summary>
        public static void SetRoundTarget(Segment targetSegment)
        {
            _currentRoundTargetSegment = targetSegment;
            _currentRoundTargetCampfireId = -1; // Will be set when we find the right campfire

            // Find the campfire that advances to this segment
            var campfires = FindObjectsByType<Campfire>(FindObjectsSortMode.None);
            foreach (var cf in campfires)
            {
                if (cf.advanceToSegment == targetSegment)
                {
                    _currentRoundTargetCampfireId = cf.GetInstanceID();
                    Plugin.Logger.LogInfo($"[Round Target] Set target campfire ID {_currentRoundTargetCampfireId} advancing to {targetSegment}");
                    break;
                }
            }

            if (_currentRoundTargetCampfireId == -1)
            {
                Plugin.Logger.LogWarning($"[Round Target] Could not find campfire advancing to {targetSegment}!");
            }
        }

        /// <summary>
        /// Get the next segment based on current segment
        /// </summary>
        public static Segment GetNextSegment(Segment current)
        {
            // Progression: Beach -> Tropics -> Alpine -> Caldera -> TheKiln -> Peak
            switch (current)
            {
                case Segment.Beach: return Segment.Tropics;
                case Segment.Tropics: return Segment.Alpine;
                case Segment.Alpine: return Segment.Caldera;
                case Segment.Caldera: return Segment.TheKiln;
                case Segment.TheKiln: return Segment.Peak;
                default: return Segment.Peak;
            }
        }

        private void Start()
        {
            // Set initial grace period
            _validDetectionTime = Time.time + GRACE_PERIOD;
        }

        private void Update()
        {
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;
            if (!MatchState.Instance.IsMatchActive) return;
            if (!MatchState.Instance.IsRoundActive) return; // Only detect during active rounds

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
        /// STRICT: Only the campfire that advances to EXACTLY the next segment is valid.
        /// </summary>
        private bool IsTargetCampfire()
        {
            // Method 1: Use explicitly set target campfire ID if available
            if (_currentRoundTargetCampfireId != -1)
            {
                bool isTarget = campfire.GetInstanceID() == _currentRoundTargetCampfireId;

                // Log once
                if (!_loggedTargetInfo && MatchState.Instance.IsRoundActive)
                {
                    Plugin.Logger.LogInfo($"[Campfire ID:{campfire.GetInstanceID()}] Target: {_currentRoundTargetCampfireId}, IsTarget: {isTarget}");
                    _loggedTargetInfo = true;
                }

                return isTarget;
            }

            // Method 2: Fallback - use segment-based detection
            Segment currentSegment = GetCurrentSegment();
            Segment expectedNextSegment = GetNextSegment(currentSegment);
            Segment campfireAdvancesTo = campfire.advanceToSegment;

            // Log target info once per campfire
            if (!_loggedTargetInfo && MatchState.Instance.IsRoundActive)
            {
                Plugin.Logger.LogInfo($"[Campfire] Current: {currentSegment}, Expected Next: {expectedNextSegment}, This advances to: {campfireAdvancesTo}");
                _loggedTargetInfo = true;
            }

            // STRICT: Only match if this campfire advances to EXACTLY the next segment
            // Not just any segment that's higher
            return campfireAdvancesTo == expectedNextSegment;
        }

        /// <summary>
        /// Get the current segment based on the match state's current map name
        /// </summary>
        private Segment GetCurrentSegment()
        {
            string mapName = MatchState.Instance.CurrentMapName?.ToLower() ?? "";

            // Map names to segments - be more specific
            if (mapName.Contains("shore") || mapName.Contains("beach") || mapName == "")
                return Segment.Beach;
            if (mapName.Contains("tropic"))
                return Segment.Tropics;
            if (mapName.Contains("mesa"))
                return Segment.Alpine; // Mesa is part of Alpine in PEAK
            if (mapName.Contains("alpine"))
                return Segment.Alpine;
            if (mapName.Contains("root"))
                return Segment.Caldera; // Roots is part of Caldera
            if (mapName.Contains("caldera"))
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
            // If we're the host, check ALL players (since only host has the mod)
            // If we're not the host, just check local player
            if (PhotonNetwork.IsMasterClient)
            {
                CheckAllPlayersProximity();
            }
            else
            {
                CheckSinglePlayerProximity();
            }
        }

        /// <summary>
        /// Host-only: Check all players in the game for campfire proximity
        /// </summary>
        private void CheckAllPlayersProximity()
        {
            Vector3 campfirePos = campfire.transform.position;

            // Iterate through ALL characters in the game
            foreach (var character in Character.AllCharacters)
            {
                if (character == null || character.view == null) continue;

                // Get the Photon player who owns this character
                var owner = character.view.Owner;
                if (owner == null) continue;

                // Skip if already detected at this campfire
                if (_detectedPlayers.Contains(owner.ActorNumber)) continue;

                // Calculate distance
                Vector3 playerPos = character.Center;
                float distance = Vector3.Distance(playerPos, campfirePos);

                // Check if within detection radius
                if (distance <= DETECTION_RADIUS)
                {
                    Plugin.Logger.LogInfo($"[Host Detection] Player {owner.ActorNumber} reached campfire! Distance: {distance:F1}m");
                    OnPlayerReachedCampfireHostDetected(owner, character);
                }
            }

            // Debug log occasionally (every 5 seconds)
            if (Time.frameCount % 300 == 0)
            {
                int playerCount = Character.AllCharacters?.Count ?? 0;
                Plugin.Logger.LogInfo($"[Host Campfire Check] Monitoring {playerCount} players for {campfire.advanceToSegment} campfire");
            }
        }

        /// <summary>
        /// Non-host: Only check local player (fallback if non-host also has mod)
        /// </summary>
        private void CheckSinglePlayerProximity()
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

        /// <summary>
        /// Called when host detects ANY player reaching the campfire (host-only detection)
        /// Since we're the host, we process directly instead of sending RPC
        /// </summary>
        private void OnPlayerReachedCampfireHostDetected(Photon.Realtime.Player player, Character character)
        {
            // Mark as detected to avoid spam
            _detectedPlayers.Add(player.ActorNumber);

            // Check if player is a ghost
            bool isGhost = character.IsGhost;

            Plugin.Logger.LogInfo($"");
            Plugin.Logger.LogInfo($"========================================");
            Plugin.Logger.LogInfo($"=== HOST DETECTED CAMPFIRE ARRIVAL ===");
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

            // Since we ARE the host, call the RPC handler directly (simulates receiving RPC)
            Plugin.Logger.LogInfo($"Processing arrival directly (we are host)...");
            NetworkSyncManager.Instance.ProcessCampfireArrival(player.ActorNumber, team.TeamId, isGhost);
        }

        /// <summary>
        /// Called when non-host player detects themselves reaching campfire
        /// Sends RPC to host for processing
        /// </summary>
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
