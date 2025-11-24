using UnityEngine;
using PEAKCompetitive.Util;

namespace PEAKCompetitive.Configuration
{
    public class RoundTimerUI : MonoBehaviour
    {
        private GUIStyle _timerStyle;
        private bool _stylesInitialized = false;

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _timerStyle = new GUIStyle(GUI.skin.label);
            _timerStyle.fontSize = 32;
            _timerStyle.fontStyle = FontStyle.Bold;
            _timerStyle.normal.textColor = Color.yellow;
            _timerStyle.alignment = TextAnchor.MiddleCenter;

            // Add shadow/outline effect
            _timerStyle.normal.textColor = Color.yellow;

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!ConfigurationHandler.EnableCompetitiveMode) return;
            if (!RoundTimerManager.Instance.IsTimerActive) return;

            InitializeStyles();

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Draw timer at top center of screen
            int minutes = RoundTimerManager.Instance.MinutesRemaining;
            int seconds = RoundTimerManager.Instance.SecondsRemaining;

            string timerText = $"{minutes:00}:{seconds:00}";

            // Change color based on time remaining
            if (minutes == 0 && seconds <= 30)
            {
                _timerStyle.normal.textColor = Color.red; // Red when < 30 seconds
            }
            else if (minutes < 2)
            {
                _timerStyle.normal.textColor = new Color(1f, 0.5f, 0f); // Orange when < 2 minutes
            }
            else
            {
                _timerStyle.normal.textColor = Color.yellow; // Yellow otherwise
            }

            float width = 200f;
            float height = 60f;
            float x = (screenWidth - width) / 2f;
            float y = 20f;

            Rect timerRect = new Rect(x, y, width, height);

            // Draw shadow
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.Label(new Rect(x + 2, y + 2, width, height), timerText, _timerStyle);

            // Draw timer
            GUI.color = Color.white;
            GUI.Label(timerRect, timerText, _timerStyle);

            // Draw "Time Remaining" label below
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(x, y + 45, width, 20), "TIME REMAINING", labelStyle);
        }
    }
}
