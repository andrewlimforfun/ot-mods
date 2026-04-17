using System;

namespace Reconnect.Core
{
    public class ReconnectManager
    {
        private float _lastSequenceTime;
        private int _currentAttempt;

        public int MaxAttempts { get; }
        public float AttemptIntervalSec { get; }
        public float CooldownSec { get; }

        public bool IsIntentionalLeave { get; set; }
        public bool IsReconnecting { get; private set; }
        public string? SavedLobbyId { get; set; }
        public int CurrentAttempt => _currentAttempt;

        public ReconnectManager(int maxAttempts = 3, float attemptIntervalSec = 5f, float cooldownSec = 30f)
        {
            MaxAttempts = Math.Clamp(maxAttempts, 1, 10);
            AttemptIntervalSec = Math.Clamp(attemptIntervalSec, 2f, 30f);
            CooldownSec = Math.Clamp(cooldownSec, 10f, 120f);
        }

        /// <summary>
        /// Attempts to begin a reconnect sequence. Returns false if cooldown is active.
        /// </summary>
        public bool TryBeginSequence(float currentTime)
        {
            if (_lastSequenceTime > 0f && (currentTime - _lastSequenceTime) < CooldownSec)
                return false;

            _lastSequenceTime = currentTime;
            _currentAttempt = 0;
            IsReconnecting = true;
            return true;
        }

        /// <summary>
        /// Advances to the next attempt. Returns false if attempts are exhausted or intentional leave.
        /// </summary>
        public bool TryNextAttempt()
        {
            if (IsIntentionalLeave)
            {
                IsReconnecting = false;
                return false;
            }

            if (_currentAttempt >= MaxAttempts)
            {
                IsReconnecting = false;
                return false;
            }

            _currentAttempt++;
            return true;
        }

        /// <summary>Called when connection succeeds.</summary>
        public void OnConnected()
        {
            IsReconnecting = false;
            IsIntentionalLeave = false;
        }

        /// <summary>Called when all attempts are exhausted.</summary>
        public void OnFailed()
        {
            IsReconnecting = false;
        }
    }
}
