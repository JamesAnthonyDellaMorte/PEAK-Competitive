#if DEBUG_MODE
using UnityEngine;
using System.Collections.Generic;

namespace PEAKCompetitive.Configuration
{
    /// <summary>
    /// In-game debug console for DEBUG builds only.
    /// Toggle with F5 key.
    /// </summary>
    public class DebugConsoleUI : MonoBehaviour
    {
        private static DebugConsoleUI _instance;
        public static DebugConsoleUI Instance => _instance;

        private bool _isVisible = false;
        private Vector2 _scrollPosition;
        private List<LogEntry> _logEntries = new List<LogEntry>();
        private const int MAX_ENTRIES = 200;

        private Rect _windowRect = new Rect(10, 10, 600, 400);
        private bool _autoScroll = true;
        private string _filterText = "";
        private bool _showInfo = true;
        private bool _showWarnings = true;
        private bool _showErrors = true;

        private GUIStyle _logStyle;
        private GUIStyle _warningStyle;
        private GUIStyle _errorStyle;
        private bool _stylesInitialized = false;

        private struct LogEntry
        {
            public string message;
            public string stackTrace;
            public LogType type;
            public float time;
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to Unity's log callback
            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            _logEntries.Add(new LogEntry
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
                time = Time.time
            });

            // Limit entries
            while (_logEntries.Count > MAX_ENTRIES)
            {
                _logEntries.RemoveAt(0);
            }

            // Auto scroll to bottom
            if (_autoScroll)
            {
                _scrollPosition.y = float.MaxValue;
            }
        }

        private void Update()
        {
            // F5 to toggle console
            if (Input.GetKeyDown(KeyCode.F5))
            {
                _isVisible = !_isVisible;
            }
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _logStyle = new GUIStyle(GUI.skin.label);
            _logStyle.normal.textColor = Color.white;
            _logStyle.fontSize = 12;
            _logStyle.wordWrap = true;

            _warningStyle = new GUIStyle(_logStyle);
            _warningStyle.normal.textColor = Color.yellow;

            _errorStyle = new GUIStyle(_logStyle);
            _errorStyle.normal.textColor = Color.red;

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            InitStyles();

            _windowRect = GUI.Window(9999, _windowRect, DrawWindow, "Debug Console (F5 to toggle)");
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.BeginVertical();

            // Filter bar
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(40));
            _filterText = GUILayout.TextField(_filterText, GUILayout.Width(150));

            _showInfo = GUILayout.Toggle(_showInfo, "Info", GUILayout.Width(50));
            _showWarnings = GUILayout.Toggle(_showWarnings, "Warn", GUILayout.Width(50));
            _showErrors = GUILayout.Toggle(_showErrors, "Error", GUILayout.Width(50));
            _autoScroll = GUILayout.Toggle(_autoScroll, "Auto-scroll", GUILayout.Width(80));

            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                _logEntries.Clear();
            }
            GUILayout.EndHorizontal();

            // Log area
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            string filterLower = _filterText.ToLower();

            foreach (var entry in _logEntries)
            {
                // Filter by type
                if (entry.type == LogType.Log && !_showInfo) continue;
                if (entry.type == LogType.Warning && !_showWarnings) continue;
                if ((entry.type == LogType.Error || entry.type == LogType.Exception) && !_showErrors) continue;

                // Filter by text
                if (!string.IsNullOrEmpty(_filterText) && !entry.message.ToLower().Contains(filterLower))
                    continue;

                GUIStyle style = _logStyle;
                string prefix = "[INFO]";

                switch (entry.type)
                {
                    case LogType.Warning:
                        style = _warningStyle;
                        prefix = "[WARN]";
                        break;
                    case LogType.Error:
                    case LogType.Exception:
                        style = _errorStyle;
                        prefix = "[ERROR]";
                        break;
                }

                string timeStr = $"[{entry.time:F1}s]";
                GUILayout.Label($"{timeStr} {prefix} {entry.message}", style);
            }

            GUILayout.EndScrollView();

            // Bottom bar with stats
            GUILayout.BeginHorizontal();
            int infoCount = 0, warnCount = 0, errorCount = 0;
            foreach (var e in _logEntries)
            {
                if (e.type == LogType.Log) infoCount++;
                else if (e.type == LogType.Warning) warnCount++;
                else errorCount++;
            }
            GUILayout.Label($"Total: {_logEntries.Count} | Info: {infoCount} | Warnings: {warnCount} | Errors: {errorCount}");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            // Make window draggable
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        /// <summary>
        /// Log a message to the debug console
        /// </summary>
        public static void Log(string message)
        {
            Debug.Log($"[PEAK-Competitive] {message}");
        }

        /// <summary>
        /// Log a warning to the debug console
        /// </summary>
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"[PEAK-Competitive] {message}");
        }

        /// <summary>
        /// Log an error to the debug console
        /// </summary>
        public static void LogError(string message)
        {
            Debug.LogError($"[PEAK-Competitive] {message}");
        }
    }
}
#endif
