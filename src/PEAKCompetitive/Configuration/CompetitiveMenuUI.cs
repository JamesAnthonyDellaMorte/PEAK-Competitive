using UnityEngine;
using Photon.Pun;

namespace PEAKCompetitive.Configuration
{
    public class CompetitiveMenuUI : MonoBehaviour
    {
        private bool _showMenu = false;
        private Rect _windowRect;
        private Vector2 _scrollPosition = Vector2.zero;
        private bool _initialized = false;

        private const string PREF_WINDOW_X = "PEAKCompetitive_MenuX";
        private const string PREF_WINDOW_Y = "PEAKCompetitive_MenuY";

        private void Start()
        {
            // Load saved position or center on screen
            float defaultX = Screen.width / 2f - 300f;
            float defaultY = Screen.height / 2f - 250f;

            float savedX = PlayerPrefs.GetFloat(PREF_WINDOW_X, defaultX);
            float savedY = PlayerPrefs.GetFloat(PREF_WINDOW_Y, defaultY);

            // Clamp to screen bounds
            savedX = Mathf.Clamp(savedX, 0, Screen.width - 600);
            savedY = Mathf.Clamp(savedY, 0, Screen.height - 500);

            _windowRect = new Rect(savedX, savedY, 600, 500);
            _initialized = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(ConfigurationHandler.MenuKey))
            {
                _showMenu = !_showMenu;

                // Save position when closing
                if (!_showMenu && _initialized)
                {
                    PlayerPrefs.SetFloat(PREF_WINDOW_X, _windowRect.x);
                    PlayerPrefs.SetFloat(PREF_WINDOW_Y, _windowRect.y);
                    PlayerPrefs.Save();
                }
            }
        }

        private void OnGUI()
        {
            if (!_showMenu || !_initialized) return;

            _windowRect = GUI.Window(0, _windowRect, DrawMenuWindow, "PEAK Competitive Settings");
        }

        private void DrawMenuWindow(int windowID)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Space(10);

            // Only host can change settings
            bool isHost = PhotonNetwork.IsMasterClient;

            if (!isHost)
            {
                GUILayout.Label("Only the host can change competitive settings");
                GUILayout.Space(10);
            }

            GUI.enabled = isHost;

            // Match Settings Section
            GUILayout.Label("=== Match Settings ===", GUI.skin.box);
            GUILayout.Space(5);

            ConfigurationHandler.EnableCompetitiveMode = GUILayout.Toggle(
                ConfigurationHandler.EnableCompetitiveMode,
                "Enable Competitive Mode"
            );

            ConfigurationHandler.ShowScoreboard = GUILayout.Toggle(
                ConfigurationHandler.ShowScoreboard,
                "Show Scoreboard"
            );

            ConfigurationHandler.ItemsPersist = GUILayout.Toggle(
                ConfigurationHandler.ItemsPersist,
                "Items Persist Between Rounds"
            );

            GUILayout.Space(10);

            // Biome Points Section - Difficulty-based scoring
            GUILayout.Label("=== Biome Point Values (Difficulty-Based) ===", GUI.skin.box);
            GUILayout.Space(5);

            ConfigurationHandler.ShorePoints = DrawPointSlider("Shore (★☆☆☆☆)", ConfigurationHandler.ShorePoints);
            ConfigurationHandler.TropicsPoints = DrawPointSlider("Tropics (★★☆☆☆)", ConfigurationHandler.TropicsPoints);
            ConfigurationHandler.MesaPoints = DrawPointSlider("Mesa (★★★☆☆)", ConfigurationHandler.MesaPoints);
            ConfigurationHandler.AlpinePoints = DrawPointSlider("Alpine (★★★★☆)", ConfigurationHandler.AlpinePoints);
            ConfigurationHandler.RootsPoints = DrawPointSlider("Roots (★★★★☆)", ConfigurationHandler.RootsPoints);
            ConfigurationHandler.CalderaPoints = DrawPointSlider("Caldera (★★★★★)", ConfigurationHandler.CalderaPoints);
            ConfigurationHandler.KilnPoints = DrawPointSlider("Kiln/Summit (★★★★★+)", ConfigurationHandler.KilnPoints);

            GUILayout.Space(10);

            // UI Settings Section
            GUILayout.Label("=== Scoreboard Position ===", GUI.skin.box);
            GUILayout.Space(5);

            GUILayout.Label($"X Position: {ConfigurationHandler.ScoreboardX:F0}");
            ConfigurationHandler.ScoreboardX = GUILayout.HorizontalSlider(
                ConfigurationHandler.ScoreboardX,
                0f,
                Screen.width
            );

            GUILayout.Label($"Y Position: {ConfigurationHandler.ScoreboardY:F0}");
            ConfigurationHandler.ScoreboardY = GUILayout.HorizontalSlider(
                ConfigurationHandler.ScoreboardY,
                0f,
                Screen.height
            );

            GUILayout.Label($"Scale: {ConfigurationHandler.ScoreboardScale:F2}");
            ConfigurationHandler.ScoreboardScale = GUILayout.HorizontalSlider(
                ConfigurationHandler.ScoreboardScale,
                0.5f,
                3.0f
            );

            GUILayout.Space(10);

            // Match Control Section
            GUILayout.Label("=== Match Control ===", GUI.skin.box);
            GUILayout.Space(5);

            if (GUILayout.Button("Start New Match"))
            {
                StartMatch();
            }

            if (GUILayout.Button("End Match"))
            {
                EndMatch();
            }

            if (GUILayout.Button("Reassign Teams"))
            {
                ReassignTeams();
            }

            GUILayout.Space(10);

            // Info Section
            GUILayout.Label("=== Current Match Info ===", GUI.skin.box);
            GUILayout.Space(5);

            var matchState = Model.MatchState.Instance;

            GUILayout.Label($"Match Active: {matchState.IsMatchActive}");
            GUILayout.Label($"Round: {matchState.CurrentRound}");
            GUILayout.Label($"Map: {matchState.CurrentMapName}");

            if (matchState.Teams.Count > 0)
            {
                GUILayout.Space(5);
                foreach (var team in matchState.Teams)
                {
                    string members = string.Join(", ", team.Members.ConvertAll(p => Util.TeamManager.GetPlayerDisplayName(p)));
                    GUILayout.Label($"{team.TeamName}: {team.Score} pts - {members}");
                }
            }

            GUI.enabled = true;

            GUILayout.Space(10);

            if (GUILayout.Button("Close"))
            {
                _showMenu = false;
            }

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        private int DrawPointSlider(string label, int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value}", GUILayout.Width(200));
            int newValue = (int)GUILayout.HorizontalSlider(value, 1, 10);
            GUILayout.EndHorizontal();
            return newValue;
        }

        private void StartMatch()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo("Starting competitive match...");

            // Assign teams
            Util.TeamManager.AssignPlayersToTeams();

            // Start match
            Model.MatchState.Instance.StartMatch();

            // Start first round on Shore
            Model.MatchState.Instance.StartRound("Shore");

            // Sync to all clients
            Util.NetworkSyncManager.Instance.SyncTeamAssignments();
            Util.NetworkSyncManager.Instance.SyncMatchStart();
            Util.NetworkSyncManager.Instance.SyncRoundStart("Shore");

            Plugin.Logger.LogInfo("Match started and synced! Round 1 active on Shore.");
        }

        private void EndMatch()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo("Ending match...");
            Model.MatchState.Instance.EndMatch();

            var winner = Model.MatchState.Instance.WinningTeam;
            if (winner != null)
            {
                Plugin.Logger.LogInfo($"{winner.TeamName} wins with {winner.Score} points!");
            }
        }

        private void ReassignTeams()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo("Reassigning teams...");
            Util.TeamManager.BalanceTeams();

            // Sync team assignments to all clients
            Util.NetworkSyncManager.Instance.SyncTeamAssignments();
        }
    }
}
