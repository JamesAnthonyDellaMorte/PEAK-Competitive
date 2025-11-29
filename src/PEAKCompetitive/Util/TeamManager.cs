using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using PEAKCompetitive.Model;
using Steamworks;

namespace PEAKCompetitive.Util
{
    public static class TeamManager
    {
        public static void AssignPlayersToTeams()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Plugin.Logger.LogWarning("Only host can assign teams");
                return;
            }

            var matchState = MatchState.Instance;
            var players = PhotonNetwork.PlayerList;

            // Free-for-all mode: each player is their own team
            if (Configuration.ConfigurationHandler.FreeForAllMode)
            {
                int playerCount = players.Length;
                matchState.InitializeTeams(playerCount, 1);

                for (int i = 0; i < players.Length; i++)
                {
                    matchState.AssignPlayerToTeam(players[i], i);
                }

                Plugin.Logger.LogInfo($"Free-for-all mode: {playerCount} players, each on their own team");
                LogTeamAssignments();
                return;
            }

            // Normal team mode
            int teamCount = Configuration.ConfigurationHandler.MaxTeams;
            int playersPerTeam = Configuration.ConfigurationHandler.PlayersPerTeam;

            // Initialize teams
            matchState.InitializeTeams(teamCount, playersPerTeam);

            // Assign players to teams in order
            int currentTeam = 0;
            foreach (var player in players)
            {
                matchState.AssignPlayerToTeam(player, currentTeam);

                // Get current team member count
                var team = matchState.Teams[currentTeam];

                // Move to next team when current team is full
                if (team.Members.Count >= playersPerTeam)
                {
                    currentTeam = (currentTeam + 1) % teamCount;
                }
            }

            LogTeamAssignments();
        }

        public static void AssignPlayerToTeamById(Photon.Realtime.Player player, int teamId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var matchState = MatchState.Instance;
            matchState.AssignPlayerToTeam(player, teamId);

            Plugin.Logger.LogInfo($"Manually assigned {player.UserId} to team {teamId}");
        }

        public static void BalanceTeams()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var matchState = MatchState.Instance;
            var players = PhotonNetwork.PlayerList;

            // Clear current teams
            foreach (var team in matchState.Teams)
            {
                team.Members.Clear();
            }

            // Redistribute evenly
            for (int i = 0; i < players.Length; i++)
            {
                int teamId = i % matchState.Teams.Count;
                matchState.AssignPlayerToTeam(players[i], teamId);
            }

            Plugin.Logger.LogInfo("Teams balanced");
            LogTeamAssignments();
        }

        public static TeamData GetPlayerTeam(Photon.Realtime.Player player)
        {
            return MatchState.Instance.GetPlayerTeam(player);
        }

        public static bool ArePlayersOnSameTeam(Photon.Realtime.Player player1, Photon.Realtime.Player player2)
        {
            var team1 = GetPlayerTeam(player1);
            var team2 = GetPlayerTeam(player2);

            return team1 != null && team2 != null && team1.TeamId == team2.TeamId;
        }

        public static List<Photon.Realtime.Player> GetTeammates(Photon.Realtime.Player player)
        {
            var team = GetPlayerTeam(player);

            if (team == null) return new List<Photon.Realtime.Player>();

            return team.Members.Where(p => p != player).ToList();
        }

        public static void RemovePlayer(Photon.Realtime.Player player)
        {
            var matchState = MatchState.Instance;

            foreach (var team in matchState.Teams)
            {
                team.RemoveMember(player);
            }

            Plugin.Logger.LogInfo($"Removed {player.UserId} from teams");
        }

        public static void ClearAllTeams()
        {
            var matchState = MatchState.Instance;

            foreach (var team in matchState.Teams)
            {
                team.Members.Clear();
            }

            Plugin.Logger.LogInfo("All teams cleared");
        }

        private static void LogTeamAssignments()
        {
            var matchState = MatchState.Instance;

            Plugin.Logger.LogInfo("=== Team Assignments ===");

            foreach (var team in matchState.Teams)
            {
                string memberNames = string.Join(", ", team.Members.Select(p => p.UserId));
                Plugin.Logger.LogInfo($"{team.TeamName}: {memberNames} ({team.Members.Count} players)");
            }

            Plugin.Logger.LogInfo("========================");
        }

        public static string GetTeamColor(int teamId)
        {
            string[] colors = { "#FF4444", "#4444FF", "#44FF44", "#FFFF44", "#FF44FF" };

            if (teamId < colors.Length)
            {
                return colors[teamId];
            }

            return "#FFFFFF";
        }

        public static string GetPlayerDisplayName(Photon.Realtime.Player player)
        {
            // Try to get Steam name from UserId
            if (!string.IsNullOrEmpty(player.UserId))
            {
                try
                {
                    // Parse Steam ID from Photon UserId
                    if (ulong.TryParse(player.UserId, out ulong steamId64))
                    {
                        CSteamID steamId = new CSteamID(steamId64);
                        string steamName = SteamFriends.GetFriendPersonaName(steamId);

                        if (!string.IsNullOrEmpty(steamName))
                        {
                            return steamName;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.LogWarning($"Failed to get Steam name for player: {ex.Message}");
                }
            }

            // Fallback to NickName if set and not a hash
            if (!string.IsNullOrEmpty(player.NickName) &&
                !player.NickName.Contains("-") &&
                player.NickName.Length < 20)
            {
                return player.NickName;
            }

            // Final fallback to "Player X" based on actor number
            return $"Player {player.ActorNumber}";
        }
    }
}
