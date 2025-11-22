using System.Collections.Generic;
using Photon.Realtime;

namespace PEAKCompetitive.Model
{
    public class TeamData
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int Score { get; set; }
        public List<Photon.Realtime.Player> Members { get; set; }
        public bool HasReachedSummit { get; set; }
        public int RoundsWon { get; set; }

        public TeamData(int teamId, string teamName)
        {
            TeamId = teamId;
            TeamName = teamName;
            Score = 0;
            Members = new List<Photon.Realtime.Player>();
            HasReachedSummit = false;
            RoundsWon = 0;
        }

        public void AddMember(Photon.Realtime.Player player)
        {
            if (!Members.Contains(player))
            {
                Members.Add(player);
            }
        }

        public void RemoveMember(Photon.Realtime.Player player)
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

        public bool IsPlayerOnTeam(Photon.Realtime.Player player)
        {
            return Members.Contains(player);
        }

        public int GetAlivePlayersCount()
        {
            // TODO: Implement check for alive (non-ghost) players
            // Will need to patch into PEAK's player state system
            // For now, return total count (placeholder)
            // Should check each player's ghost status:
            // int aliveCount = 0;
            // foreach (var player in Members) {
            //     if (!IsGhost(player)) aliveCount++;
            // }
            // return aliveCount;
            return Members.Count;
        }

        public int GetGhostPlayersCount()
        {
            // TODO: Count ghosts in team
            // return Members.Count - GetAlivePlayersCount();
            return 0;
        }
    }
}
