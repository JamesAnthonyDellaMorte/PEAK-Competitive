using Photon.Pun;
using Photon.Realtime;
using PEAKCompetitive.Model;
using PEAKCompetitive.Configuration;
using System.Linq;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

namespace PEAKCompetitive.Util
{
    public class NetworkSyncManager : MonoBehaviourPunCallbacks
    {
        private const string PROP_MATCH_ACTIVE = "MatchActive";
        private const string PROP_ROUND_ACTIVE = "RoundActive";
        private const string PROP_ROUND = "Round";
        private const string PROP_MAP_NAME = "MapName";
        private const string PROP_TEAM_ASSIGNMENTS = "TeamAssignments";
        private const string PROP_TEAM_SCORES = "TeamScores";
        private const string PROP_TIMER_ACTIVE = "TimerActive";
        private const string PROP_TIMER_REMAINING = "TimerRemaining";

        private static NetworkSyncManager _instance;
        private static Character _localCharacter;
        private PhotonView _photonView;

        public static NetworkSyncManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new UnityEngine.GameObject("NetworkSyncManager");
                    _instance = go.AddComponent<NetworkSyncManager>();
                    _instance._photonView = go.AddComponent<PhotonView>();
                    _instance._photonView.ViewID = 999; // Fixed ViewID for NetworkSyncManager
                    UnityEngine.Object.DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            // Register with Photon callbacks
            PhotonNetwork.AddCallbackTarget(this);
            Plugin.Logger.LogInfo("NetworkSyncManager registered with Photon");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            // Unregister from Photon callbacks
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void SetLocalCharacter(Character character)
        {
            _localCharacter = character;
            Plugin.Logger.LogInfo("Local character set for NetworkSyncManager");
        }

        // Sync match state to room properties
        public void SyncMatchStart()
        {
            if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
            {
                Plugin.Logger.LogWarning("Cannot sync match start - not master client or no room");
                return;
            }

            var props = new Hashtable
            {
                { PROP_MATCH_ACTIVE, true }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            Plugin.Logger.LogInfo("Synced match start to room properties");
        }

        // Sync team assignments
        public void SyncTeamAssignments()
        {
            if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null) return;

            var matchState = MatchState.Instance;

            // Serialize team assignments as "teamId:actorNum1,actorNum2;teamId:..."
            var teamData = new List<string>();
            var scoreData = new List<string>();

            foreach (var team in matchState.Teams)
            {
                var actorNums = string.Join(",", team.Members.Select(p => p.ActorNumber));
                teamData.Add($"{team.TeamId}:{actorNums}");
                scoreData.Add($"{team.TeamId}:{team.Score}");
            }

            var props = new ExitGames.Client.Photon.Hashtable
            {
                { PROP_TEAM_ASSIGNMENTS, string.Join(";", teamData) },
                { PROP_TEAM_SCORES, string.Join(";", scoreData) }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            Plugin.Logger.LogInfo($"Synced teams: {string.Join(";", teamData)}");
            Plugin.Logger.LogInfo($"Synced scores: {string.Join(";", scoreData)}");
        }

        // Handle room property changes
        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Plugin.Logger.LogInfo($"OnRoomPropertiesUpdate called with {propertiesThatChanged.Count} properties");

            var matchState = MatchState.Instance;

            // Match active state changed
            if (propertiesThatChanged.ContainsKey(PROP_MATCH_ACTIVE))
            {
                bool isActive = (bool)propertiesThatChanged[PROP_MATCH_ACTIVE];
                matchState.IsMatchActive = isActive;
                Plugin.Logger.LogInfo($"Match active updated: {isActive}");
            }

            // Round active state changed
            if (propertiesThatChanged.ContainsKey(PROP_ROUND_ACTIVE))
            {
                bool roundActive = (bool)propertiesThatChanged[PROP_ROUND_ACTIVE];
                matchState.IsRoundActive = roundActive;
                Plugin.Logger.LogInfo($"Round active updated: {roundActive}");
            }

            // Team assignments changed
            if (propertiesThatChanged.ContainsKey(PROP_TEAM_ASSIGNMENTS))
            {
                string teamData = (string)propertiesThatChanged[PROP_TEAM_ASSIGNMENTS];
                ApplyTeamAssignments(teamData);
            }

            // Team scores changed
            if (propertiesThatChanged.ContainsKey(PROP_TEAM_SCORES))
            {
                string scoreData = (string)propertiesThatChanged[PROP_TEAM_SCORES];
                ApplyTeamScores(scoreData);
            }

            // Timer state changed
            if (propertiesThatChanged.ContainsKey(PROP_TIMER_ACTIVE))
            {
                bool timerActive = (bool)propertiesThatChanged[PROP_TIMER_ACTIVE];
                float timeRemaining = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(PROP_TIMER_REMAINING)
                    ? (float)PhotonNetwork.CurrentRoom.CustomProperties[PROP_TIMER_REMAINING]
                    : 600f;

                RoundTimerManager.Instance.SetTimerActive(timerActive, timeRemaining);
            }

            // Map/round changed
            if (propertiesThatChanged.ContainsKey(PROP_MAP_NAME))
            {
                string mapName = (string)propertiesThatChanged[PROP_MAP_NAME];
                matchState.CurrentMapName = mapName;
                Plugin.Logger.LogInfo($"Map changed to: {mapName}");
            }

            if (propertiesThatChanged.ContainsKey(PROP_ROUND))
            {
                int round = (int)propertiesThatChanged[PROP_ROUND];
                matchState.CurrentRound = round;
                Plugin.Logger.LogInfo($"Round changed to: {round}");
            }
        }

        private void ApplyTeamAssignments(string teamData)
        {
            if (string.IsNullOrEmpty(teamData)) return;

            var matchState = MatchState.Instance;

            // Initialize teams if needed
            if (matchState.Teams.Count == 0)
            {
                matchState.InitializeTeams(ConfigurationHandler.MaxTeams, ConfigurationHandler.PlayersPerTeam);
            }

            Plugin.Logger.LogInfo($"Applying team assignments: {teamData}");

            // Parse "teamId:actorNum1,actorNum2;teamId:..."
            string[] teams = teamData.Split(';');
            foreach (string teamInfo in teams)
            {
                if (string.IsNullOrEmpty(teamInfo)) continue;

                string[] parts = teamInfo.Split(':');
                if (parts.Length != 2) continue;

                int teamId = int.Parse(parts[0]);
                string[] actorNumStrs = parts[1].Split(',');

                var team = matchState.Teams.FirstOrDefault(t => t.TeamId == teamId);
                if (team != null)
                {
                    team.Members.Clear();
                    foreach (string actorNumStr in actorNumStrs)
                    {
                        if (string.IsNullOrEmpty(actorNumStr)) continue;

                        int actorNum = int.Parse(actorNumStr);
                        Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNum);
                        if (player != null)
                        {
                            team.AddMember(player);
                            Plugin.Logger.LogInfo($"Assigned Player {actorNum} to {team.TeamName}");
                        }
                    }
                }
            }
        }

        private void ApplyTeamScores(string scoreData)
        {
            if (string.IsNullOrEmpty(scoreData)) return;

            var matchState = MatchState.Instance;

            bool isHost = PhotonNetwork.IsMasterClient;
            string clientType = isHost ? "[HOST]" : "[CLIENT]";
            Plugin.Logger.LogInfo($"{clientType} Applying team scores: {scoreData}");

            // Parse "teamId:score;teamId:score;..."
            string[] scores = scoreData.Split(';');
            foreach (string scoreInfo in scores)
            {
                if (string.IsNullOrEmpty(scoreInfo)) continue;

                string[] parts = scoreInfo.Split(':');
                if (parts.Length != 2) continue;

                int teamId = int.Parse(parts[0]);
                int score = int.Parse(parts[1]);

                var team = matchState.Teams.FirstOrDefault(t => t.TeamId == teamId);
                if (team != null)
                {
                    team.Score = score;
                    Plugin.Logger.LogInfo($"{clientType} Updated {team.TeamName} score to {score} (MatchState updated!)");
                }
            }

            Plugin.Logger.LogInfo($"{clientType} Score sync complete - ScoreboardUI should now display these scores");
        }

        // Sync timer start
        public void SyncTimerStart()
        {
            if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null) return;

            var props = new Hashtable
            {
                { PROP_TIMER_ACTIVE, true },
                { PROP_TIMER_REMAINING, 600f } // 10 minutes
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            Plugin.Logger.LogInfo("Synced timer start to all clients");
        }

        // Sync round start with map name
        public void SyncRoundStart(string mapName)
        {
            if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null) return;

            var props = new Hashtable
            {
                { PROP_ROUND_ACTIVE, true },
                { PROP_MAP_NAME, mapName },
                { PROP_ROUND, MatchState.Instance.CurrentRound }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            Plugin.Logger.LogInfo($"Synced round start: Round {MatchState.Instance.CurrentRound} on {mapName}");
        }

        // Sync kill all players
        public void SyncKillAllPlayers()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Plugin.Logger.LogInfo("Syncing kill all players event");

            if (_photonView != null)
            {
                _photonView.RPC("RPC_KillAllPlayers", RpcTarget.All);
            }
        }

        [PunRPC]
        private void RPC_KillAllPlayers()
        {
            Plugin.Logger.LogInfo("RPC_KillAllPlayers received - killing local character");

            var localChar = CharacterHelper.GetLocalCharacter();
            if (localChar != null)
            {
                CharacterHelper.KillCharacter(localChar);
            }
        }

        // Sync revive all players
        public void SyncReviveAllPlayers(Vector3? teleportPosition)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Plugin.Logger.LogInfo("Syncing revive all players event");

            if (_photonView != null)
            {
                // Serialize position as float array or send null flag
                if (teleportPosition.HasValue)
                {
                    float[] pos = new float[] { teleportPosition.Value.x, teleportPosition.Value.y, teleportPosition.Value.z };
                    _photonView.RPC("RPC_ReviveAllPlayers", RpcTarget.All, pos);
                }
                else
                {
                    _photonView.RPC("RPC_ReviveAllPlayers", RpcTarget.All, null);
                }
            }
        }

        [PunRPC]
        private void RPC_ReviveAllPlayers(float[] position)
        {
            Plugin.Logger.LogInfo("RPC_ReviveAllPlayers received - reviving local character");

            var localChar = CharacterHelper.GetLocalCharacter();
            if (localChar != null)
            {
                CharacterHelper.ReviveCharacter(localChar);

                // Teleport if position provided
                if (position != null && position.Length == 3)
                {
                    Vector3 teleportPos = new Vector3(position[0], position[1], position[2]);
                    CharacterHelper.TeleportCharacter(localChar, teleportPos);
                    Plugin.Logger.LogInfo($"Teleported to {teleportPos}");
                }
            }
        }

        // RPC for non-host players to notify host of campfire arrival
        public void NotifyHostOfCampfireArrival(int playerActorNumber, int teamId, bool isGhost)
        {
            if (_photonView == null)
            {
                Plugin.Logger.LogError("PhotonView is null! Cannot send RPC.");
                return;
            }

            Plugin.Logger.LogInfo($"Sending RPC to host: Player {playerActorNumber} from team {teamId} reached campfire (ghost: {isGhost})");
            _photonView.RPC("RPC_PlayerReachedCampfire", RpcTarget.MasterClient, playerActorNumber, teamId, isGhost);
        }

        [PunRPC]
        private void RPC_PlayerReachedCampfire(int playerActorNumber, int teamId, bool isGhost)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo($"=== RPC RECEIVED: CAMPFIRE ARRIVAL ===");
            Plugin.Logger.LogInfo($"Player {playerActorNumber} from team {teamId} reached campfire (ghost: {isGhost})");

            var matchState = MatchState.Instance;
            var team = matchState.Teams.FirstOrDefault(t => t.TeamId == teamId);

            if (team == null)
            {
                Plugin.Logger.LogWarning($"Team {teamId} not found!");
                return;
            }

            // Check if player already reached
            if (team.PlayersWhoReached.Contains(playerActorNumber))
            {
                Plugin.Logger.LogInfo($"Player {playerActorNumber} already reached - ignoring duplicate");
                return;
            }

            // Record the arrival with ghost status
            team.RecordArrival(playerActorNumber, isGhost);
            Plugin.Logger.LogInfo($"Player {playerActorNumber} from {team.TeamName} reached! ({team.PlayersWhoReached.Count}/{team.Members.Count}, ghost: {isGhost})");

            // Check if this is the first player from this team (to set placement)
            if (team.FinishPlacement == 0)
            {
                // First member arrived - determine placement
                int placement = GetNextPlacement();
                team.FinishPlacement = placement;
                team.HasReachedSummit = true;

                Plugin.Logger.LogInfo($"{team.TeamName} is #{placement} to reach the campfire!");

                // Start timer when first team arrives
                if (placement == 1)
                {
                    Plugin.Logger.LogInfo("First team arrived - starting 10-minute timer!");
                    RoundTimerManager.Instance.StartTimer();
                }
            }

            // Count non-ghost members who reached (using tracked ghost status at arrival time)
            int nonGhostArrivals = team.GetNonGhostArrivals();
            Plugin.Logger.LogInfo($"{team.TeamName}: {nonGhostArrivals} non-ghost members reached, {team.GhostPlayersWhoReached.Count} ghosts (ghosts don't count)");

            // Calculate and award points immediately for this arrival (if not a ghost)
            if (!isGhost)
            {
                int baseMapPoints = Configuration.ConfigurationHandler.GetMapPoints(matchState.CurrentMapName);
                float placementMultiplier = GetPlacementMultiplier(team.FinishPlacement);
                int pointsForThisPlayer = (int)(baseMapPoints * placementMultiplier);

                // Use Score directly instead of AddScore to avoid incrementing RoundsWon multiple times
                team.Score += pointsForThisPlayer;
                Plugin.Logger.LogInfo($"=== POINTS AWARDED ===");
                Plugin.Logger.LogInfo($"{team.TeamName} +{pointsForThisPlayer} points! (Placement #{team.FinishPlacement}: {placementMultiplier:F1}x × {baseMapPoints} base)");
                Plugin.Logger.LogInfo($"{team.TeamName} new total: {team.Score} points");
            }
            else
            {
                Plugin.Logger.LogWarning($"Player {playerActorNumber} is a ghost - 0 points awarded!");
            }

            // Sync scores to all clients immediately
            Plugin.Logger.LogInfo("Syncing team scores to all clients...");
            SyncTeamAssignments();

            // Check if all teams have finished
            if (AllTeamsFinished())
            {
                Plugin.Logger.LogInfo("All teams finished! Ending round early...");
                RoundTimerManager.Instance.StopTimer();
                RoundTransitionManager.Instance.StartTransition();
            }
        }

        /// <summary>
        /// Get the next available placement (1st, 2nd, 3rd, etc.)
        /// </summary>
        private int GetNextPlacement()
        {
            var matchState = MatchState.Instance;
            int maxPlacement = 0;

            foreach (var team in matchState.Teams)
            {
                if (team.FinishPlacement > maxPlacement)
                {
                    maxPlacement = team.FinishPlacement;
                }
            }

            return maxPlacement + 1;
        }

        /// <summary>
        /// Get point multiplier based on placement
        /// 1st place = 1.0x (100%)
        /// 2nd place = 0.7x (70%)
        /// 3rd place = 0.5x (50%)
        /// 4th+ place = 0.3x (30%)
        /// </summary>
        private float GetPlacementMultiplier(int placement)
        {
            switch (placement)
            {
                case 1: return 1.0f;  // 1st place: full points
                case 2: return 0.7f;  // 2nd place: 70%
                case 3: return 0.5f;  // 3rd place: 50%
                default: return 0.3f; // 4th+: 30%
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
