using PEAKCompetitive.Model;
using System.Linq;

namespace PEAKCompetitive.Util
{
    public static class ScoringCalculator
    {
        /// <summary>
        /// Calculate total points for a team reaching checkpoint
        /// </summary>
        /// <param name="team">The winning team</param>
        /// <param name="basePoints">Base points for the map</param>
        /// <param name="livingMembersCount">Number of team members who reached checkpoint alive</param>
        /// <returns>Total points to award</returns>
        public static float CalculateRoundPoints(TeamData team, int basePoints, int livingMembersCount)
        {
            float totalPoints = 0f;

            // 1. Base points for winning the round
            totalPoints += basePoints;
            Plugin.Logger.LogInfo($"  Base points: {basePoints}");

            // 2. Individual completion bonus (per living member)
            float individualMultiplier = Configuration.ConfigurationHandler.IndividualCompletionMultiplier;
            if (individualMultiplier > 0 && livingMembersCount > 0)
            {
                float individualBonus = basePoints * individualMultiplier * livingMembersCount;
                totalPoints += individualBonus;
                Plugin.Logger.LogInfo($"  Individual bonus: {basePoints} × {individualMultiplier} × {livingMembersCount} = {individualBonus}");
            }

            // 3. Full team bonus (all members alive)
            bool allTeamAlive = livingMembersCount >= team.Members.Count;
            if (Configuration.ConfigurationHandler.EnableFullTeamBonus && allTeamAlive)
            {
                float fullTeamBonus = basePoints;
                totalPoints += fullTeamBonus;
                Plugin.Logger.LogInfo($"  Full team bonus: {fullTeamBonus} (all {team.Members.Count} members alive!)");
            }
            else if (livingMembersCount < team.Members.Count)
            {
                int deadMembers = team.Members.Count - livingMembersCount;
                Plugin.Logger.LogInfo($"  No full team bonus ({deadMembers} member(s) died)");
            }

            Plugin.Logger.LogInfo($"  TOTAL POINTS: {totalPoints}");

            return totalPoints;
        }

        /// <summary>
        /// Calculate points breakdown for display
        /// </summary>
        public static string GetPointsBreakdown(int basePoints, int livingMembers, int totalMembers)
        {
            float total = 0f;
            string breakdown = "";

            // Base
            total += basePoints;
            breakdown += $"{basePoints} (base)";

            // Individual
            float individualMultiplier = Configuration.ConfigurationHandler.IndividualCompletionMultiplier;
            if (individualMultiplier > 0 && livingMembers > 0)
            {
                float individual = basePoints * individualMultiplier * livingMembers;
                total += individual;
                breakdown += $" + {individual:F1} ({livingMembers} alive)";
            }

            // Full team
            if (Configuration.ConfigurationHandler.EnableFullTeamBonus && livingMembers >= totalMembers)
            {
                total += basePoints;
                breakdown += $" + {basePoints} (full team!)";
            }

            return $"{breakdown} = {total:F1} pts";
        }

        /// <summary>
        /// Get potential points that were lost due to deaths
        /// </summary>
        public static float GetLostPoints(int basePoints, int livingMembers, int totalMembers)
        {
            if (livingMembers >= totalMembers) return 0f; // No one died

            float lostPoints = 0f;

            // Lost individual points
            int deadMembers = totalMembers - livingMembers;
            float individualMultiplier = Configuration.ConfigurationHandler.IndividualCompletionMultiplier;
            lostPoints += basePoints * individualMultiplier * deadMembers;

            // Lost full team bonus
            if (Configuration.ConfigurationHandler.EnableFullTeamBonus)
            {
                lostPoints += basePoints;
            }

            return lostPoints;
        }

        /// <summary>
        /// Calculate points for "all teams dead" scenario
        /// </summary>
        public static float CalculateAllDeadPoints(int basePoints)
        {
            // Only award base points, no bonuses
            return basePoints;
        }
    }
}
