using Photon.Pun;
using Photon.Realtime;
using PEAKCompetitive.Model;
using PEAKCompetitive.Configuration;
using System.Linq;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace PEAKCompetitive.Util
{
    public class NetworkSyncManager : MonoBehaviourPunCallbacks
    {
        private const string PROP_MATCH_ACTIVE = "MatchActive";
        private const string PROP_ROUND = "Round";
        private const string PROP_MAP_NAME = "MapName";
        private const string PROP_TEAM_ASSIGNMENTS = "TeamAssignments";
        private const string PROP_TEAM_SCORES = "TeamScores";
        private const string PROP_TIMER_ACTIVE = "TimerActive";
        private const string PROP_TIMER_REMAINING = "TimerRemaining";

        private static NetworkSyncManager _instance;
        private static Character _localCharacter;

        public static NetworkSyncManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new UnityEngine.GameObject("NetworkSyncManager");
                    _instance = go.AddComponent<NetworkSyncManager>();
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
            Plugin.Logger.LogInfo($"Applying team scores: {scoreData}");

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
                    Plugin.Logger.LogInfo($"Updated {team.TeamName} score to {score}");
                }
            }
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
                { PROP_MAP_NAME, mapName },
                { PROP_ROUND, MatchState.Instance.CurrentRound }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            Plugin.Logger.LogInfo($"Synced round start: {mapName}");
        }

        // Sync kill all players
        public void SyncKillAllPlayers()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Plugin.Logger.LogInfo("Syncing kill all players event");
            // TODO: Implement via RPC or custom property
        }

        // Sync revive all players
        public void SyncReviveAllPlayers()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Plugin.Logger.LogInfo("Syncing revive all players event");
            // TODO: Implement via RPC or custom property
        }
    }
}
