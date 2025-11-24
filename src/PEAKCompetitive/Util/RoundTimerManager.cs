using UnityEngine;
using PEAKCompetitive.Model;
using Photon.Pun;

namespace PEAKCompetitive.Util
{
    public class RoundTimerManager : MonoBehaviour
    {
        private static RoundTimerManager _instance;
        private float _roundTimeRemaining;
        private bool _timerActive;
        private const float ROUND_DURATION = 600f; // 10 minutes in seconds

        public static RoundTimerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("RoundTimerManager");
                    _instance = go.AddComponent<RoundTimerManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public float TimeRemaining => _roundTimeRemaining;
        public bool IsTimerActive => _timerActive;
        public int MinutesRemaining => Mathf.FloorToInt(_roundTimeRemaining / 60f);
        public int SecondsRemaining => Mathf.FloorToInt(_roundTimeRemaining % 60f);

        public void StartTimer()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            _roundTimeRemaining = ROUND_DURATION;
            _timerActive = true;

            Plugin.Logger.LogInfo($"Round timer started: {ROUND_DURATION} seconds");

            // Sync timer start to all clients
            NetworkSyncManager.Instance.SyncTimerStart();
        }

        public void SetTimerActive(bool active, float timeRemaining)
        {
            _timerActive = active;
            _roundTimeRemaining = timeRemaining;

            if (active)
            {
                Plugin.Logger.LogInfo($"Timer synced: {timeRemaining:F1}s remaining");
            }
        }

        private void Update()
        {
            if (!_timerActive) return;

            _roundTimeRemaining -= Time.deltaTime;

            if (_roundTimeRemaining <= 0f)
            {
                _roundTimeRemaining = 0f;
                _timerActive = false;

                if (PhotonNetwork.IsMasterClient)
                {
                    Plugin.Logger.LogInfo("Round timer expired!");
                    OnTimerExpired();
                }
            }
        }

        private void OnTimerExpired()
        {
            // End the round and transition
            var matchState = MatchState.Instance;

            Plugin.Logger.LogInfo("Time's up! Ending round and transitioning...");

            // Determine winner (team with most points or first to finish)
            var leadingTeam = matchState.GetLeadingTeam();

            if (leadingTeam != null)
            {
                Plugin.Logger.LogInfo($"{leadingTeam.TeamName} wins the round!");
                matchState.EndRound(leadingTeam, 0); // Points already awarded at campfire
            }

            // Trigger round transition (kill/revive/teleport)
            RoundTransitionManager.Instance.StartTransition();
        }

        public void StopTimer()
        {
            _timerActive = false;
            _roundTimeRemaining = 0f;
        }
    }
}
