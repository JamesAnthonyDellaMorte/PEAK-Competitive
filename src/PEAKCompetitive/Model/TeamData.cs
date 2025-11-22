using System.Collections.Generic;
using Photon.Realtime;

namespace PEAKCompetitive.Model
{
    public class TeamData
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int Score { get; set; }
        public List<Player> Members { get; set; }
        public bool HasReachedSummit { get; set; }
        public int RoundsWon { get; set; }

        public TeamData(int teamId, string teamName)
        {
            TeamId = teamId;
            TeamName = teamName;
            Score = 0;
            Members = new List<Player>();
            HasReachedSummit = false;
            RoundsWon = 0;
        }

        public void AddMember(Player player)
        {
            if (!Members.Contains(player))
            {
                Members.Add(player);
            }
        }

        public void RemoveMember(Player player)
        {
            Members.Remove(player);
        }

        public void AddScore(int points)
        {
            Score += points;
            RoundsWon++;
        }

        public void ResetRoundState()
        {
            HasReachedSummit = false;
        }

        public void ResetMatch()
        {
            Score = 0;
            RoundsWon = 0;
            HasReachedSummit = false;
        }

        public bool IsPlayerOnTeam(Player player)
        {
            return Members.Contains(player);
        }

        public int GetAlivePlayersCount()
        {
            // TODO: Implement check for alive players
            // Will need to patch into PEAK's player state system
            return Members.Count;
        }
    }
}
