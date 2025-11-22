using UnityEngine;
using PEAKCompetitive.Model;
using System.Linq;

namespace PEAKCompetitive.Configuration
{
    public class ScoreboardUI : MonoBehaviour
    {
        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _teamStyle;
        private GUIStyle _scoreStyle;
        private bool _stylesInitialized = false;

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            // Box style
            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.9f));
            _boxStyle.padding = new RectOffset(10, 10, 10, 10);

            // Header style
            _headerStyle = new GUIStyle(GUI.skin.label);
            _headerStyle.fontSize = 20;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.normal.textColor = Color.white;
            _headerStyle.alignment = TextAnchor.MiddleCenter;

            // Team style
            _teamStyle = new GUIStyle(GUI.skin.label);
            _teamStyle.fontSize = 16;
            _teamStyle.fontStyle = FontStyle.Bold;
            _teamStyle.normal.textColor = Color.white;
            _teamStyle.padding = new RectOffset(5, 5, 5, 5);

            // Score style
            _scoreStyle = new GUIStyle(GUI.skin.label);
            _scoreStyle.fontSize = 18;
            _scoreStyle.fontStyle = FontStyle.Bold;
            _scoreStyle.normal.textColor = Color.yellow;
            _scoreStyle.alignment = TextAnchor.MiddleRight;

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!ConfigurationHandler.ShowScoreboard) return;
            if (!MatchState.Instance.IsMatchActive) return;

            InitializeStyles();

            float scale = ConfigurationHandler.ScoreboardScale;
            Matrix4x4 originalMatrix = GUI.matrix;

            // Apply scaling
            GUI.matrix = Matrix4x4.TRS(
                new Vector3(ConfigurationHandler.ScoreboardX, ConfigurationHandler.ScoreboardY, 0),
                Quaternion.identity,
                new Vector3(scale, scale, 1)
            );

            DrawScoreboard();

            GUI.matrix = originalMatrix;
        }

        private void DrawScoreboard()
        {
            float width = 300f;
            float baseHeight = 100f;
            float teamHeight = 60f;

            var matchState = MatchState.Instance;
            int teamCount = matchState.Teams.Count;

            float totalHeight = baseHeight + (teamCount * teamHeight);

            GUILayout.BeginArea(new Rect(0, 0, width, totalHeight), _boxStyle);

            // Header
            GUILayout.Label("PEAK COMPETITIVE", _headerStyle);
            GUILayout.Space(5);

            // Round info
            GUILayout.Label($"Round {matchState.CurrentRound} - {matchState.CurrentMapName}", _teamStyle);
            GUILayout.Space(10);

            // Team scores (sorted by score)
            var sortedTeams = matchState.Teams.OrderByDescending(t => t.Score).ToList();

            foreach (var team in sortedTeams)
            {
                DrawTeamScore(team);
            }

            GUILayout.EndArea();
        }

        private void DrawTeamScore(TeamData team)
        {
            GUILayout.BeginHorizontal();

            // Team indicator box
            Color teamColor = HexToColor(Util.TeamManager.GetTeamColor(team.TeamId));
            GUIStyle colorBox = new GUIStyle(GUI.skin.box);
            colorBox.normal.background = MakeTexture(2, 2, teamColor);

            GUILayout.Box("", colorBox, GUILayout.Width(20), GUILayout.Height(20));

            // Team name and member count
            GUILayout.Label($"{team.TeamName} ({team.Members.Count})", _teamStyle, GUILayout.Width(150));

            GUILayout.FlexibleSpace();

            // Score
            GUILayout.Label($"{team.Score} pts", _scoreStyle, GUILayout.Width(80));

            GUILayout.EndHorizontal();

            // Player names
            string playerNames = string.Join(", ", team.Members.Select(p => p.UserId));
            GUILayout.Label($"  {playerNames}", GUI.skin.label);

            GUILayout.Space(5);
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
