using BepInEx.Configuration;
using UnityEngine;

namespace PEAKCompetitive.Configuration
{
    public static class ConfigurationHandler
    {
        private static ConfigFile _config;

        // General Settings
        public static bool EnableCompetitiveMode { get; private set; }
        public static KeyCode MenuKey { get; private set; }

        // Team Settings
        public static int MaxTeams { get; private set; }
        public static int PlayersPerTeam { get; private set; }

        // Match Settings
        public static bool ItemsPersist { get; private set; }
        public static bool ShowScoreboard { get; private set; }

        // Map Point Values (host configurable)
        public static int Map1Points { get; set; }
        public static int Map2Points { get; set; }
        public static int Map3Points { get; set; }
        public static int Map4Points { get; set; }
        public static int RuthsMapPoints { get; set; }

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

            // Map Point Values
            var map1PointsEntry = config.Bind(
                "MapPoints",
                "Map1Points",
                1,
                new ConfigDescription(
                    "Points awarded for winning Map 1",
                    new AcceptableValueRange<int>(1, 10)
                )
            );
            Map1Points = map1PointsEntry.Value;

            var map2PointsEntry = config.Bind(
                "MapPoints",
                "Map2Points",
                1,
                new ConfigDescription(
                    "Points awarded for winning Map 2",
                    new AcceptableValueRange<int>(1, 10)
                )
            );
            Map2Points = map2PointsEntry.Value;

            var map3PointsEntry = config.Bind(
                "MapPoints",
                "Map3Points",
                2,
                new ConfigDescription(
                    "Points awarded for winning Map 3",
                    new AcceptableValueRange<int>(1, 10)
                )
            );
            Map3Points = map3PointsEntry.Value;

            var map4PointsEntry = config.Bind(
                "MapPoints",
                "Map4Points",
                2,
                new ConfigDescription(
                    "Points awarded for winning Map 4",
                    new AcceptableValueRange<int>(1, 10)
                )
            );
            Map4Points = map4PointsEntry.Value;

            var ruthsMapPointsEntry = config.Bind(
                "MapPoints",
                "RuthsMapPoints",
                3,
                new ConfigDescription(
                    "Points awarded for winning Ruth's Map",
                    new AcceptableValueRange<int>(1, 10)
                )
            );
            RuthsMapPoints = ruthsMapPointsEntry.Value;

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
            Plugin.Logger.LogInfo($"Map Points: M1={Map1Points}, M2={Map2Points}, M3={Map3Points}, M4={Map4Points}, Ruth's={RuthsMapPoints}");
            Plugin.Logger.LogInfo("=====================================");
        }

        public static int GetMapPoints(string mapName)
        {
            // PEAK biome names: Shore, Tropics, Roots, Alpine, Mesa, Caldera, Kiln, Summit
            // Map1 = Shore (easy tutorial)
            // Map2 = Tropics/Roots (medium jungle/forest)
            // Map3 = Alpine/Mesa (hard snow/desert)
            // Map4 = Caldera (very hard volcano slopes)
            // RuthsMap = Kiln (extreme inner volcano)

            string mapLower = mapName.ToLower();

            if (mapLower.Contains("shore") || mapLower.Contains("coast") || mapLower.Contains("beach"))
                return Map1Points;

            if (mapLower.Contains("tropics") || mapLower.Contains("jungle") || mapLower.Contains("roots") || mapLower.Contains("redwood"))
                return Map2Points;

            if (mapLower.Contains("alpine") || mapLower.Contains("snow") || mapLower.Contains("mesa") || mapLower.Contains("desert"))
                return Map3Points;

            if (mapLower.Contains("caldera") || mapLower.Contains("volcano"))
                return Map4Points;

            if (mapLower.Contains("kiln") || mapLower.Contains("summit") || mapLower.Contains("peak"))
                return RuthsMapPoints;

            // Default to 1 point if map not recognized
            return 1;
        }
    }
}
