using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using PEAKCompetitive.Util;

namespace PEAKCompetitive.Patches
{
    // Handle player join/leave for team management
    // From PEAK Unlimited, we know PlayerConnectionLog exists

    [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
    public class OnPlayerEnteredRoomPatch
    {
        static void Postfix(Photon.Realtime.Player newPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (!Configuration.ConfigurationHandler.EnableCompetitiveMode) return;

            Plugin.Logger.LogInfo($"Player joined: {newPlayer.UserId}");

            // Assign to team if match is not active
            var matchState = Model.MatchState.Instance;

            if (!matchState.IsMatchActive)
            {
                // Auto-assign to team with fewest players
                var smallestTeam = GetSmallestTeam();

                if (smallestTeam != null)
                {
                    matchState.AssignPlayerToTeam(newPlayer, smallestTeam.TeamId);
                    Plugin.Logger.LogInfo($"Auto-assigned {newPlayer.UserId} to {smallestTeam.TeamName}");
                }
            }
            else
            {
                Plugin.Logger.LogWarning($"Match in progress - {newPlayer.UserId} not assigned to team");
            }
        }

        private static Model.TeamData GetSmallestTeam()
        {
            var matchState = Model.MatchState.Instance;

            if (matchState.Teams.Count == 0) return null;

            Model.TeamData smallestTeam = matchState.Teams[0];

            foreach (var team in matchState.Teams)
            {
                if (team.Members.Count < smallestTeam.Members.Count)
                {
                    smallestTeam = team;
                }
            }

            return smallestTeam;
        }
    }

    [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerLeftRoom")]
    public class OnPlayerLeftRoomPatch
    {
        static void Postfix(Photon.Realtime.Player newPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo($"Player left: {newPlayer.UserId}");

            // Remove from team
            TeamManager.RemovePlayer(newPlayer);

            // Check if match should end (not enough players)
            var matchState = Model.MatchState.Instance;

            if (matchState.IsMatchActive)
            {
                int totalPlayers = PhotonNetwork.PlayerList.Length;
                int minPlayers = Configuration.ConfigurationHandler.MaxTeams *
                                Configuration.ConfigurationHandler.PlayersPerTeam;

                if (totalPlayers < minPlayers)
                {
                    Plugin.Logger.LogWarning($"Not enough players for competitive match ({totalPlayers}/{minPlayers})");
                    // Could optionally end match here
                }
            }
        }
    }
}
