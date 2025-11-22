using UnityEngine;
using PEAKCompetitive.Model;

namespace PEAKCompetitive.Configuration
{
    public class MatchNotificationUI : MonoBehaviour
    {
        private string _notificationText = "";
        private float _notificationTime = 0f;
        private float _notificationDuration = 5f;
        private Color _notificationColor = Color.white;

        private GUIStyle _notificationStyle;
        private bool _stylesInitialized = false;

        public static MatchNotificationUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _notificationStyle = new GUIStyle();
            _notificationStyle.fontSize = 40;
            _notificationStyle.fontStyle = FontStyle.Bold;
            _notificationStyle.alignment = TextAnchor.MiddleCenter;
            _notificationStyle.normal.textColor = Color.white;

            // Add shadow/outline effect
            _notificationStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
            _notificationStyle.padding = new RectOffset(20, 20, 10, 10);

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!ConfigurationHandler.EnableCompetitiveMode) return;
            if (_notificationTime <= 0) return;

            InitializeStyles();

            // Calculate fade out
            float alpha = Mathf.Clamp01(_notificationTime / 1f);
            _notificationStyle.normal.textColor = new Color(
                _notificationColor.r,
                _notificationColor.g,
                _notificationColor.b,
                alpha
            );

            // Center of screen
            float width = 800f;
            float height = 100f;
            float x = Screen.width / 2 - width / 2;
            float y = Screen.height / 3;

            GUI.Label(new Rect(x, y, width, height), _notificationText, _notificationStyle);

            _notificationTime -= Time.deltaTime;
        }

        public void ShowRoundWin(TeamData winningTeam, int points)
        {
            _notificationText = $"{winningTeam.TeamName} wins the round!\n+{points} points";
            _notificationColor = HexToColor(Util.TeamManager.GetTeamColor(winningTeam.TeamId));
            _notificationTime = _notificationDuration;

            Plugin.Logger.LogInfo($"Showing round win notification: {_notificationText}");
        }

        public void ShowMatchWin(TeamData winningTeam)
        {
            _notificationText = $"{winningTeam.TeamName} WINS THE MATCH!\nFinal Score: {winningTeam.Score} points";
            _notificationColor = Color.yellow;
            _notificationTime = _notificationDuration * 2; // Show longer for match win

            Plugin.Logger.LogInfo($"Showing match win notification: {_notificationText}");
        }

        public void ShowMatchStart()
        {
            _notificationText = "COMPETITIVE MATCH STARTING!";
            _notificationColor = Color.cyan;
            _notificationTime = _notificationDuration;

            Plugin.Logger.LogInfo("Showing match start notification");
        }

        public void ShowRoundStart(int roundNumber, string mapName)
        {
            _notificationText = $"Round {roundNumber}\n{mapName}";
            _notificationColor = Color.white;
            _notificationTime = 3f;

            Plugin.Logger.LogInfo($"Showing round start notification: Round {roundNumber}");
        }

        public void ShowAllTeamsDead()
        {
            _notificationText = "ALL TEAMS ELIMINATED!\nRound Draw";
            _notificationColor = Color.red;
            _notificationTime = _notificationDuration;

            Plugin.Logger.LogInfo("Showing all teams dead notification");
        }

        public void ShowCustomNotification(string text, Color color, float duration = 5f)
        {
            _notificationText = text;
            _notificationColor = color;
            _notificationTime = duration;
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}
