using UnityEngine;
using Photon.Pun;

namespace PEAKCompetitive.Configuration
{
    public class CompetitiveMenuUI : MonoBehaviour
    {
        private bool _visible = false;
        private int _selectedIndex = 0;

        private Texture2D _whiteTex;
        private GUIStyle _titleStyle;
        private GUIStyle _rowStyle;
        private GUIStyle _hintStyle;
        private GUIStyle _infoStyle;

        private const string TITLE = "PEAK Competitive";
        private const string HINT = "F3: Open/Close • ↑/↓: Navigate • Enter: Toggle/Activate";

        private int RowHeight = 28;
        private int PanelWidth = 400;
        private int Pad = 12;
        private int TitleFontSize = 20;
        private int OptionFontSize = 14;
        private int HintFontSize = 12;

        private string[] _menuOptions = new string[]
        {
            "Enable Competitive Mode",
            "Show Scoreboard",
            "Free-for-All Mode",
            "Start Match",
            "End Match",
            "Reassign Teams"
        };

        private void Update()
        {
            if (Input.GetKeyDown(ConfigurationHandler.MenuKey))
            {
                _visible = !_visible;
                if (_visible) OnOpened();
                else OnClosed();
            }

            if (!_visible) return;

            // Keyboard navigation
            if (Input.GetKeyDown(KeyCode.UpArrow))
                _selectedIndex = (_selectedIndex - 1 + _menuOptions.Length) % _menuOptions.Length;
            if (Input.GetKeyDown(KeyCode.DownArrow))
                _selectedIndex = (_selectedIndex + 1) % _menuOptions.Length;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                ActivateOption(_selectedIndex);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _visible = false;
                OnClosed();
            }
        }

        private void OnOpened()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnClosed()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

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
                fontSize = TitleFontSize,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            _rowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = OptionFontSize,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 4, 4),
                normal = { textColor = Color.white }
            };

            _hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = HintFontSize,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };

            _infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = HintFontSize,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.8f, 0.9f, 1f) }
            };
        }

        private void OnGUI()
        {
            if (!_visible) return;
            EnsureStyles();

            bool isHost = PhotonNetwork.IsMasterClient;
            var matchState = Model.MatchState.Instance;

            // Calculate panel height
            int optionCount = _menuOptions.Length;
            int infoLines = 3 + (matchState.Teams?.Count ?? 0);
            int panelHeight = Pad + 30 + 8 + (optionCount * (RowHeight + 2)) + 20 + (infoLines * 20) + 30 + Pad;

            Rect panelRect = new Rect(20, 20, PanelWidth, panelHeight);

            // Draw background
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(panelRect, _whiteTex);
            GUI.color = Color.white;

            float y = panelRect.y + Pad;

            // Title
            GUI.Label(new Rect(panelRect.x + Pad, y, PanelWidth - Pad * 2, 30), TITLE, _titleStyle);
            y += 38;

            // Host warning
            if (!isHost)
            {
                GUI.color = new Color(1f, 0.6f, 0.6f);
                GUI.Label(new Rect(panelRect.x + Pad, y, PanelWidth - Pad * 2, 20), "Only host can change settings", _hintStyle);
                GUI.color = Color.white;
                y += 24;
            }

            // Menu options
            for (int i = 0; i < _menuOptions.Length; i++)
            {
                Rect rowRect = new Rect(panelRect.x + Pad, y, PanelWidth - Pad * 2, RowHeight);

                // Hover detection
                if (rowRect.Contains(Event.current.mousePosition))
                    _selectedIndex = i;

                // Selection highlight
                if (i == _selectedIndex)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    GUI.DrawTexture(rowRect, _whiteTex);
                    GUI.color = Color.white;
                }

                string label = GetOptionLabel(i);
                bool enabled = isHost || i < 3; // Non-hosts can see toggles but not activate buttons

                GUI.enabled = enabled;
                if (GUI.Button(rowRect, label, _rowStyle))
                {
                    if (enabled) ActivateOption(i);
                }
                GUI.enabled = true;

                y += RowHeight + 2;
            }

            y += 12;

            // Separator
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.DrawTexture(new Rect(panelRect.x + Pad, y, PanelWidth - Pad * 2, 1), _whiteTex);
            GUI.color = Color.white;
            y += 8;

            // Match info
            GUI.Label(new Rect(panelRect.x + Pad, y, PanelWidth - Pad * 2, 20),
                $"Match: {(matchState.IsMatchActive ? "Active" : "Inactive")} | Round: {matchState.CurrentRound}", _infoStyle);
            y += 20;

            if (matchState.Teams != null && matchState.Teams.Count > 0)
            {
                foreach (var team in matchState.Teams)
                {
                    string members = string.Join(", ", team.Members.ConvertAll(p => Util.TeamManager.GetPlayerDisplayName(p)));
                    GUI.Label(new Rect(panelRect.x + Pad, y, PanelWidth - Pad * 2, 20),
                        $"{team.TeamName}: {team.Score} pts - {members}", _infoStyle);
                    y += 20;
                }
            }

            y += 8;

            // Hint
            GUI.Label(new Rect(panelRect.x + Pad, y, PanelWidth - Pad * 2, 24), HINT, _hintStyle);
        }

        private string GetOptionLabel(int index)
        {
            switch (index)
            {
                case 0: return $"Competitive Mode: {(ConfigurationHandler.EnableCompetitiveMode ? "ON" : "OFF")}";
                case 1: return $"Show Scoreboard: {(ConfigurationHandler.ShowScoreboard ? "ON" : "OFF")}";
                case 2: return $"Free-for-All: {(ConfigurationHandler.FreeForAllMode ? "ON" : "OFF")}";
                case 3: return "► Start Match";
                case 4: return "■ End Match";
                case 5: return "↻ Reassign Teams";
                default: return _menuOptions[index];
            }
        }

        private void ActivateOption(int index)
        {
            switch (index)
            {
                case 0:
                    ConfigurationHandler.EnableCompetitiveMode = !ConfigurationHandler.EnableCompetitiveMode;
                    break;
                case 1:
                    ConfigurationHandler.ShowScoreboard = !ConfigurationHandler.ShowScoreboard;
                    break;
                case 2:
                    ConfigurationHandler.FreeForAllMode = !ConfigurationHandler.FreeForAllMode;
                    break;
                case 3:
                    StartMatch();
                    break;
                case 4:
                    EndMatch();
                    break;
                case 5:
                    ReassignTeams();
                    break;
            }
        }

        private void StartMatch()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo("Starting competitive match...");
            Util.TeamManager.AssignPlayersToTeams();
            Model.MatchState.Instance.StartMatch();
            Model.MatchState.Instance.StartRound("Shore");
            Util.NetworkSyncManager.Instance.SyncTeamAssignments();
            Util.NetworkSyncManager.Instance.SyncMatchStart();
            Util.NetworkSyncManager.Instance.SyncRoundStart("Shore");
            Plugin.Logger.LogInfo("Match started!");
        }

        private void EndMatch()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo("Ending match...");
            Model.MatchState.Instance.EndMatch();
        }

        private void ReassignTeams()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Plugin.Logger.LogInfo("Reassigning teams...");
            Util.TeamManager.BalanceTeams();
            Util.NetworkSyncManager.Instance.SyncTeamAssignments();
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
