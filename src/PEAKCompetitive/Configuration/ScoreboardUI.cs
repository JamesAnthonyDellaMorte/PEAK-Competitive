using UnityEngine;
using PEAKCompetitive.Model;
using System.Linq;
using Photon.Pun;

namespace PEAKCompetitive.Configuration
{
    public class ScoreboardUI : MonoBehaviour
    {
        private Texture2D _whiteTex;
        private GUIStyle _titleStyle;
        private GUIStyle _teamStyle;
        private GUIStyle _scoreStyle;
        private GUIStyle _infoStyle;

        private const int WIDTH = 280;
        private const int PAD = 10;

        private void EnsureStyles()
        {
            if (_whiteTex == null)
            {
                _whiteTex = new Texture2D(1, 1);
                _whiteTex.SetPixel(0, 0, Color.white);
                _whiteTex.Apply();
            }

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            _teamStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            _scoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.white }
            };

            _infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.8f, 1f) }
            };
        }

        private void OnGUI()
        {
            if (!ConfigurationHandler.ShowScoreboard) return;

            var matchState = MatchState.Instance;
            if (!matchState.IsMatchActive || matchState.Teams == null || matchState.Teams.Count == 0) return;

            EnsureStyles();

            int teamCount = matchState.Teams.Count;
            int height = PAD + 24 + 8 + (teamCount * 50) + 8 + 20 + PAD;

            // Position in top-right corner
            Rect panelRect = new Rect(Screen.width - WIDTH - 20, 20, WIDTH, height);

            // Background
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(panelRect, _whiteTex);
            GUI.color = Color.white;

            float y = panelRect.y + PAD;

            // Title
            GUI.Label(new Rect(panelRect.x, y, WIDTH, 24), "PEAK COMPETITIVE", _titleStyle);
            y += 32;

            // Teams sorted by score
            var sortedTeams = matchState.Teams.OrderByDescending(t => t.Score).ToList();

            foreach (var team in sortedTeams)
            {
                // Team color bar
                GUI.color = GetTeamColor(team.TeamId);
                GUI.DrawTexture(new Rect(panelRect.x + PAD, y, 4, 40), _whiteTex);
                GUI.color = Color.white;

                // Team name and members
                string members = string.Join(", ", team.Members.ConvertAll(p => Util.TeamManager.GetPlayerDisplayName(p)));
                GUI.Label(new Rect(panelRect.x + PAD + 12, y + 2, WIDTH - 80, 20), team.TeamName, _teamStyle);

                var memberStyle = new GUIStyle(_teamStyle) { fontSize = 10, normal = { textColor = new Color(0.8f, 0.8f, 0.8f) } };
                GUI.Label(new Rect(panelRect.x + PAD + 12, y + 20, WIDTH - 80, 18), members, memberStyle);

                // Score
                GUI.Label(new Rect(panelRect.x + WIDTH - 70, y + 8, 60, 30), team.Score.ToString(), _scoreStyle);

                y += 50;
            }

            y += 4;

            // Round info
            GUI.Label(new Rect(panelRect.x, y, WIDTH, 20), $"Round {matchState.CurrentRound} • {matchState.CurrentMapName}", _infoStyle);

            // Show nickname info for host
            if (PhotonNetwork.IsMasterClient)
            {
                y += 20;
                var nickStyle = new GUIStyle(_infoStyle) { fontSize = 9, normal = { textColor = new Color(0.5f, 0.7f, 0.5f) } };
                GUI.Label(new Rect(panelRect.x, y, WIDTH, 16), $"Others see: {PhotonNetwork.LocalPlayer.NickName}", nickStyle);
            }
        }

        private Color GetTeamColor(int teamId)
        {
            switch (teamId)
            {
                case 0: return new Color(0.9f, 0.3f, 0.3f);  // Red
                case 1: return new Color(0.3f, 0.5f, 0.9f);  // Blue
                case 2: return new Color(0.3f, 0.8f, 0.3f);  // Green
                case 3: return new Color(0.9f, 0.8f, 0.2f);  // Yellow
                case 4: return new Color(0.7f, 0.3f, 0.9f);  // Purple
                default: return Color.white;
            }
        }

        private void OnDestroy()
        {
            if (_whiteTex != null)
            {
                Destroy(_whiteTex);
                _whiteTex = null;
            }
        }
    }
}
