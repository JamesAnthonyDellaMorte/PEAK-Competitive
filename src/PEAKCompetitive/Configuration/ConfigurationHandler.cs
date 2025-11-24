using BepInEx.Configuration;
using UnityEngine;

namespace PEAKCompetitive.Configuration
{
    public static class ConfigurationHandler
    {
        private static ConfigFile _config;

        // General Settings
        public static bool EnableCompetitiveMode { get; set; }
        public static KeyCode MenuKey { get; private set; }

        // Team Settings
        public static int MaxTeams { get; private set; }
        public static int PlayersPerTeam { get; private set; }

        // Match Settings
        public static bool ItemsPersist { get; set; }
        public static bool ShowScoreboard { get; set; }
        public static float IndividualCompletionMultiplier { get; private set; }
        public static bool EnableFullTeamBonus { get; private set; }

        // Biome Point Values (host configurable) - Based on difficulty
        public static int ShorePoints { get; set; }      // ★☆☆☆☆ Easy
        public static int TropicsPoints { get; set; }    // ★★☆☆☆ Moderate
        public static int MesaPoints { get; set; }       // ★★★☆☆ Moderate+
        public static int AlpinePoints { get; set; }     // ★★★★☆ Hard
        public static int RootsPoints { get; set; }      // ★★★★☆ Hard
        public static int CalderaPoints { get; set; }    // ★★★★★ Very Hard
        public static int KilnPoints { get; set; }       // ★★★★★+ Extreme

        // UI Settings
        public static float ScoreboardX { get; set; }
        public static float ScoreboardY { get; set; }
        public static float ScoreboardScale { get; set; }

        // Debug
        public static bool EnableDebugLogging { get; private set; }

        public static void Initialize(ConfigFile config)
        {
            _config = config;

            // General Settings
            var enableCompetitiveEntry = config.Bind(
                "General",
                "EnableCompetitiveMode",
                true,
                "Enable competitive duo team race mode"
            );
            EnableCompetitiveMode = enableCompetitiveEntry.Value;

            var menuKeyEntry = config.Bind(
                "General",
                "MenuKey",
                KeyCode.F3,
                "Key to open competitive settings menu"
            );
            MenuKey = menuKeyEntry.Value;

            // Team Settings
            var maxTeamsEntry = config.Bind(
                "Teams",
                "MaxTeams",
                2,
                new ConfigDescription(
                    "Maximum number of teams (2 = 2v2, 3 = 2v2v2, etc.)",
                    new AcceptableValueRange<int>(2, 10)
                )
            );
            MaxTeams = maxTeamsEntry.Value;

            var playersPerTeamEntry = config.Bind(
                "Teams",
                "PlayersPerTeam",
                2,
                new ConfigDescription(
                    "Players per team",
                    new AcceptableValueRange<int>(1, 4)
                )
            );
            PlayersPerTeam = playersPerTeamEntry.Value;

            // Match Settings
            var itemsPersistEntry = config.Bind(
                "Match",
                "ItemsPersist",
                true,
                "Items persist between rounds for all players"
            );
            ItemsPersist = itemsPersistEntry.Value;

            var showScoreboardEntry = config.Bind(
                "Match",
                "ShowScoreboard",
                true,
                "Display scoreboard during match"
            );
            ShowScoreboard = showScoreboardEntry.Value;

            var individualCompletionMultiplierEntry = config.Bind(
                "Match",
                "IndividualCompletionMultiplier",
                0.5f,
                new ConfigDescription(
                    "Points multiplier per living team member (0 = disabled, 0.5 = half points per member, 1.0 = full points per member)",
                    new AcceptableValueRange<float>(0f, 2.0f)
                )
            );
            IndividualCompletionMultiplier = individualCompletionMultiplierEntry.Value;

            var enableFullTeamBonusEntry = config.Bind(
                "Match",
                "EnableFullTeamBonus",
                true,
                "Award bonus points when entire team reaches checkpoint alive (bonus = base map points)"
            );
            EnableFullTeamBonus = enableFullTeamBonusEntry.Value;

            // Biome Point Values - Difficulty-based scoring
            var shorePointsEntry = config.Bind(
                "BiomePoints",
                "ShorePoints",
                1,
                new ConfigDescription(
                    "Points for Shore biome (★☆☆☆☆ Easy)",
                    new AcceptableValueRange<int>(1, 20)
                )
            );
            ShorePoints = shorePointsEntry.Value;

            var tropicsPointsEntry = config.Bind(
                "BiomePoints",
                "TropicsPoints",
                2,
                new ConfigDescription(
                    "Points for Tropics biome (★★☆☆☆ Moderate)",
                    new AcceptableValueRange<int>(1, 20)
                )
            );
            TropicsPoints = tropicsPointsEntry.Value;

            var mesaPointsEntry = config.Bind(
                "BiomePoints",
                "MesaPoints",
                3,
                new ConfigDescription(
                    "Points for Mesa biome (★★★☆☆ Moderate+)",
                    new AcceptableValueRange<int>(1, 20)
                )
            );
            MesaPoints = mesaPointsEntry.Value;

            var alpinePointsEntry = config.Bind(
                "BiomePoints",
                "AlpinePoints",
                4,
                new ConfigDescription(
                    "Points for Alpine biome (★★★★☆ Hard)",
                    new AcceptableValueRange<int>(1, 20)
                )
            );
            AlpinePoints = alpinePointsEntry.Value;

            var rootsPointsEntry = config.Bind(
                "BiomePoints",
                "RootsPoints",
                4,
                new ConfigDescription(
                    "Points for Roots biome (★★★★☆ Hard)",
                    new AcceptableValueRange<int>(1, 20)
                )
            );
            RootsPoints = rootsPointsEntry.Value;

            var calderaPointsEntry = config.Bind(
                "BiomePoints",
                "CalderaPoints",
                5,
                new ConfigDescription(
                    "Points for Caldera biome (★★★★★ Very Hard)",
                    new AcceptableValueRange<int>(1, 20)
                )
            );
            CalderaPoints = calderaPointsEntry.Value;

            var kilnPointsEntry = config.Bind(
                "BiomePoints",
                "KilnPoints",
                6,
                new ConfigDescription(
                    "Points for Kiln/Summit (★★★★★+ Extreme)",
                    new AcceptableValueRange<int>(1, 20)
                )
            );
            KilnPoints = kilnPointsEntry.Value;

            // UI Settings
            var scoreboardXEntry = config.Bind(
                "UI",
                "ScoreboardX",
                10f,
                new ConfigDescription(
                    "Scoreboard X position (pixels from left)",
                    new AcceptableValueRange<float>(0f, 1920f)
                )
            );
            ScoreboardX = scoreboardXEntry.Value;

            var scoreboardYEntry = config.Bind(
                "UI",
                "ScoreboardY",
                10f,
                new ConfigDescription(
                    "Scoreboard Y position (pixels from top)",
                    new AcceptableValueRange<float>(0f, 1080f)
                )
            );
            ScoreboardY = scoreboardYEntry.Value;

            var scoreboardScaleEntry = config.Bind(
                "UI",
                "ScoreboardScale",
                1.0f,
                new ConfigDescription(
                    "Scoreboard scale multiplier",
                    new AcceptableValueRange<float>(0.5f, 3.0f)
                )
            );
            ScoreboardScale = scoreboardScaleEntry.Value;

            // Debug
            var enableDebugLoggingEntry = config.Bind(
                "Debug",
                "EnableDebugLogging",
                false,
                "Enable detailed debug logging"
            );
            EnableDebugLogging = enableDebugLoggingEntry.Value;

            Plugin.Logger.LogInfo("Configuration loaded successfully!");
            LogConfiguration();
        }

        private static void LogConfiguration()
        {
            Plugin.Logger.LogInfo("=== PEAK Competitive Configuration ===");
            Plugin.Logger.LogInfo($"Competitive Mode: {EnableCompetitiveMode}");
            Plugin.Logger.LogInfo($"Teams: {MaxTeams} teams of {PlayersPerTeam}");
            Plugin.Logger.LogInfo($"Items Persist: {ItemsPersist}");
            Plugin.Logger.LogInfo($"Individual Bonus: {IndividualCompletionMultiplier}x per survivor");
            Plugin.Logger.LogInfo($"Full Team Bonus: {EnableFullTeamBonus}");
            Plugin.Logger.LogInfo($"Biome Points: Shore={ShorePoints}, Tropics={TropicsPoints}, Mesa={MesaPoints}, Alpine={AlpinePoints}, Roots={RootsPoints}, Caldera={CalderaPoints}, Kiln={KilnPoints}");
            Plugin.Logger.LogInfo("=====================================");
        }

        public static int GetMapPoints(string mapName)
        {
            // PEAK biome difficulty progression:
            // Shore (1★) → Tropics (2★) → Mesa (3★) → Alpine (4★) → Roots (4★) → Caldera (5★) → Kiln (5★+)

            string mapLower = mapName.ToLower();

            // Shore - Easy tutorial beach (1 point)
            if (mapLower.Contains("shore") || mapLower.Contains("coast") || mapLower.Contains("beach"))
                return ShorePoints;

            // Tropics - Moderate jungle (2 points)
            if (mapLower.Contains("tropics") || mapLower.Contains("jungle") || mapLower.Contains("tropical"))
                return TropicsPoints;

            // Mesa - Moderate+ desert (3 points)
            if (mapLower.Contains("mesa") || mapLower.Contains("desert") || mapLower.Contains("cactus"))
                return MesaPoints;

            // Alpine - Hard snowy mountain (4 points)
            if (mapLower.Contains("alpine") || mapLower.Contains("snow") || mapLower.Contains("ice"))
                return AlpinePoints;

            // Roots - Hard dark forest (4 points)
            if (mapLower.Contains("roots") || mapLower.Contains("redwood") || mapLower.Contains("forest"))
                return RootsPoints;

            // Caldera - Very hard volcano slopes (5 points)
            if (mapLower.Contains("caldera") || mapLower.Contains("volcano"))
                return CalderaPoints;

            // Kiln/Summit - Extreme inner volcano (6 points)
            if (mapLower.Contains("kiln") || mapLower.Contains("summit") || mapLower.Contains("peak"))
                return KilnPoints;

            // Default to Shore points if biome not recognized
            Plugin.Logger.LogWarning($"Unknown biome '{mapName}', defaulting to {ShorePoints} points");
            return ShorePoints;
        }
    }
}
