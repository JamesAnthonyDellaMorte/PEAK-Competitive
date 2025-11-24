using PEAKCompetitive.Model;
using System.Linq;

namespace PEAKCompetitive.Util
{
    /// <summary>
    /// Handles all scoring calculations for competitive matches.
    ///
    /// SCORING FORMULA:
    /// Total = Base + (Base × Multiplier × Survivors) + Full Team Bonus
    ///
    /// Example: Alpine (4 pts), 2-player team, 0.5 multiplier
    /// - 2/2 survive: 4 + (4×0.5×2) + 4 = 12 points  (300% of base!)
    /// - 1/2 survive: 4 + (4×0.5×1) + 0 =  6 points  (150% of base)
    /// - 0/2 survive: 4 + 0 + 0         =  4 points  (100% of base, ghosts only)
    ///
    /// MORE SURVIVORS = MORE POINTS!
    /// </summary>
    public static class ScoringCalculator
    {
        /// <summary>
        /// Calculate total points for a team reaching checkpoint
        /// </summary>
        /// <param name="team">The winning team</param>
        /// <param name="basePoints">Base points for the biome difficulty</param>
        /// <param name="livingMembersCount">Number of team members who reached checkpoint alive</param>
        /// <returns>Total points to award (Base + Survivor bonuses)</returns>
        public static float CalculateRoundPoints(TeamData team, int basePoints, int livingMembersCount)
        {
            float totalPoints = 0f;

            // 1. Base points for winning the round
            totalPoints += basePoints;
            Plugin.Logger.LogInfo($"  Base points: {basePoints}");

            // 2. Survivor bonus (per living member) - MORE SURVIVORS = MORE POINTS!
            float individualMultiplier = Configuration.ConfigurationHandler.IndividualCompletionMultiplier;
            if (individualMultiplier > 0 && livingMembersCount > 0)
            {
                float individualBonus = basePoints * individualMultiplier * livingMembersCount;
                totalPoints += individualBonus;
                Plugin.Logger.LogInfo($"  Survivor bonus: {basePoints} × {individualMultiplier} × {livingMembersCount} survivors = +{individualBonus} pts");
            }
            else if (individualMultiplier > 0)
            {
                Plugin.Logger.LogInfo($"  Survivor bonus: 0 (no survivors, team ghosted to checkpoint)");
            }

            // 3. Full team bonus (all members alive) - KEEP EVERYONE ALIVE!
            bool allTeamAlive = livingMembersCount >= team.Members.Count;
            if (Configuration.ConfigurationHandler.EnableFullTeamBonus && allTeamAlive)
            {
                float fullTeamBonus = basePoints;
                totalPoints += fullTeamBonus;
                Plugin.Logger.LogInfo($"  Full team bonus: +{fullTeamBonus} pts (all {team.Members.Count} members alive!)");
            }
            else if (Configuration.ConfigurationHandler.EnableFullTeamBonus && livingMembersCount < team.Members.Count)
            {
                int deadMembers = team.Members.Count - livingMembersCount;
                float lostBonus = basePoints;
                Plugin.Logger.LogInfo($"  Full team bonus: 0 ({deadMembers} died, lost {lostBonus} pts)");
            }

            Plugin.Logger.LogInfo($"  TOTAL POINTS: {totalPoints} ({(int)((totalPoints / basePoints) * 100)}% of base)");

            return totalPoints;
        }

        /// <summary>
        /// Calculate points breakdown for display - Shows how survivor count affects scoring
        /// </summary>
        public static string GetPointsBreakdown(int basePoints, int livingMembers, int totalMembers)
        {
            float total = 0f;
            string breakdown = "";

            // Base points
            total += basePoints;
            breakdown += $"{basePoints} base";

            // Survivor bonus - scales with number of living members
            float individualMultiplier = Configuration.ConfigurationHandler.IndividualCompletionMultiplier;
            if (individualMultiplier > 0 && livingMembers > 0)
            {
                float individual = basePoints * individualMultiplier * livingMembers;
                total += individual;
                breakdown += $" + {individual:F1} ({livingMembers}/{totalMembers} survived)";
            }
            else if (individualMultiplier > 0 && livingMembers == 0)
            {
                breakdown += $" + 0 (ghosted)";
            }

            // Full team bonus - only if everyone survived
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
