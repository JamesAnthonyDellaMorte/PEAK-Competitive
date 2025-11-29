using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using PhotonPlayer = Photon.Realtime.Player;

namespace PEAKCompetitive.Model
{
    public class MatchState
    {
        public List<TeamData> Teams { get; private set; }
        public bool IsMatchActive { get; set; }
        public bool IsRoundActive { get; set; }
        public int CurrentRound { get; set; }
        public string CurrentMapName { get; set; }
        public TeamData WinningTeam { get; private set; }

        /// <summary>
        /// Tracks global player arrival order (1st player, 2nd player, etc.)
        /// Reset at start of each round.
        /// </summary>
        public int NextArrivalPlacement { get; set; }

        private static MatchState _instance;

        public static MatchState Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MatchState();
                }
                return _instance;
            }
        }

        private MatchState()
        {
            Teams = new List<TeamData>();
            IsMatchActive = false;
            IsRoundActive = false;
            CurrentRound = 0;
            CurrentMapName = "";
            NextArrivalPlacement = 1;
        }

        public void InitializeTeams(int teamCount, int playersPerTeam)
        {
            Teams.Clear();

            for (int i = 0; i < teamCount; i++)
            {
                string teamName = GetTeamName(i);
                Teams.Add(new TeamData(i, teamName));
            }

            Plugin.Logger.LogInfo($"Initialized {teamCount} teams with {playersPerTeam} players each");
        }

        private string GetTeamName(int teamIndex)
        {
            string[] teamNames = { "Red Team", "Blue Team", "Green Team", "Yellow Team", "Purple Team" };

            if (teamIndex < teamNames.Length)
            {
                return teamNames[teamIndex];
            }

            return $"Team {teamIndex + 1}";
        }

        public void AssignPlayerToTeam(Photon.Realtime.Player player, int teamId)
        {
            var team = Teams.FirstOrDefault(t => t.TeamId == teamId);

            if (team != null)
            {
                team.AddMember(player);
                Plugin.Logger.LogInfo($"Assigned {player.UserId} to {team.TeamName}");
            }
        }

        public TeamData GetPlayerTeam(Photon.Realtime.Player player)
        {
            return Teams.FirstOrDefault(t => t.IsPlayerOnTeam(player));
        }

        public void StartMatch()
        {
            IsMatchActive = true;
            CurrentRound = 0;

            foreach (var team in Teams)
            {
                team.ResetMatch();
            }

            Plugin.Logger.LogInfo("Match started!");

            // Show match start notification
            var notificationUI = Configuration.MatchNotificationUI.Instance;
            if (notificationUI != null)
            {
                notificationUI.ShowMatchStart();
            }
        }

        public void StartRound(string mapName)
        {
            IsRoundActive = true;
            CurrentRound++;
            CurrentMapName = mapName;
            NextArrivalPlacement = 1; // Reset for new round

            foreach (var team in Teams)
            {
                team.ResetRoundState();
            }

            Plugin.Logger.LogInfo($"Round {CurrentRound} started on {mapName}");

            // Show round start notification
            var notificationUI = Configuration.MatchNotificationUI.Instance;
            if (notificationUI != null)
            {
                notificationUI.ShowRoundStart(CurrentRound, mapName);
            }
        }

        public void EndRound(TeamData winningTeam, int points)
        {
            IsRoundActive = false;

            if (winningTeam != null)
            {
                winningTeam.AddScore(points);
                Plugin.Logger.LogInfo($"{winningTeam.TeamName} won round {CurrentRound} for {points} points! Total: {winningTeam.Score}");

                // Show round win notification
                var notificationUI = Configuration.MatchNotificationUI.Instance;
                if (notificationUI != null)
                {
                    notificationUI.ShowRoundWin(winningTeam, points);
                }
            }
            else
            {
                // All teams dead
                var notificationUI = Configuration.MatchNotificationUI.Instance;
                if (notificationUI != null)
                {
                    notificationUI.ShowAllTeamsDead();
                }
            }
        }

        public void EndMatch()
        {
            IsMatchActive = false;
            IsRoundActive = false;

            WinningTeam = GetLeadingTeam();

            if (WinningTeam != null)
            {
                Plugin.Logger.LogInfo($"Match ended! {WinningTeam.TeamName} wins with {WinningTeam.Score} points!");

                // Show match win notification
                var notificationUI = Configuration.MatchNotificationUI.Instance;
                if (notificationUI != null)
                {
                    notificationUI.ShowMatchWin(WinningTeam);
                }
            }
        }

        public TeamData GetLeadingTeam()
        {
            return Teams.OrderByDescending(t => t.Score).FirstOrDefault();
        }

        public bool CheckMatchEndConditions()
        {
            // Check if all teams have reached the Kiln (end game)
            // Or if all teams are dead
            // TODO: Implement actual checks with game state
            return false;
        }

        public void TeamReachedSummit(TeamData team, int livingMembersCount)
        {
            team.HasReachedSummit = true;

            int baseMapPoints = Configuration.ConfigurationHandler.GetMapPoints(CurrentMapName);

            // Calculate total points including individual and team bonuses
            float totalPoints = Util.ScoringCalculator.CalculateRoundPoints(team, baseMapPoints, livingMembersCount);

            Plugin.Logger.LogInfo($"{team.TeamName} reached checkpoint!");
            Plugin.Logger.LogInfo($"Living members: {livingMembersCount}/{team.Members.Count}");
            Plugin.Logger.LogInfo($"Scoring breakdown:");

            // Log lost points if any members died
            float lostPoints = Util.ScoringCalculator.GetLostPoints(baseMapPoints, livingMembersCount, team.Members.Count);
            if (lostPoints > 0)
            {
                Plugin.Logger.LogWarning($"  Lost {lostPoints:F1} points due to {team.Members.Count - livingMembersCount} death(s)!");
            }

            EndRound(team, (int)totalPoints);
        }

        public bool AllTeamsDead()
        {
            // TODO: Check if all players from all teams are dead
            return Teams.All(t => t.GetAlivePlayersCount() == 0);
        }

        public void Reset()
        {
            Teams.Clear();
            IsMatchActive = false;
            IsRoundActive = false;
            CurrentRound = 0;
            WinningTeam = null;
        }
    }
}
