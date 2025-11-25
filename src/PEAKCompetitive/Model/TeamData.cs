using System.Collections.Generic;
using Photon.Realtime;
using PEAKCompetitive.Util;

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
        public int FinishPlacement { get; set; } // 1st, 2nd, 3rd team to finish
        public HashSet<int> PlayersWhoReached { get; set; } // Track which players reached (by ActorNumber)

        public TeamData(int teamId, string teamName)
        {
            TeamId = teamId;
            TeamName = teamName;
            Score = 0;
            Members = new List<Photon.Realtime.Player>();
            HasReachedSummit = false;
            RoundsWon = 0;
            FinishPlacement = 0;
            PlayersWhoReached = new HashSet<int>();
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
            FinishPlacement = 0;
            PlayersWhoReached.Clear();
        }

        public void ResetMatch()
        {
            Score = 0;
            RoundsWon = 0;
            HasReachedSummit = false;
            FinishPlacement = 0;
            PlayersWhoReached.Clear();
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

        /// <summary>
        /// Get count of non-ghost players who reached the campfire
        /// </summary>
        public int GetNonGhostArrivals()
        {
            int count = 0;
            foreach (int actorNumber in PlayersWhoReached)
            {
                // Find the character for this actor number
                var character = CharacterHelper.GetCharacterByActorNumber(actorNumber);
                if (character != null && !character.IsGhost)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
