using System;

namespace Reconnect.Core
{
    public class ReconnectManager
    {
        private float _lastSequenceTime;
        private int _currentAttempt;

        private readonly Func<int> _maxAttempts;
        private readonly Func<float> _attemptIntervalSec;
        private readonly Func<float> _cooldownSec;

        public int MaxAttempts => _maxAttempts();
        public float AttemptIntervalSec => _attemptIntervalSec();
        public float CooldownSec => _cooldownSec();

        public bool IsIntentionalLeave { get; set; }
        public bool IsReconnecting { get; private set; }
        public string? SavedLobbyId { get; set; }
        public int CurrentAttempt => _currentAttempt;

        public ReconnectManager(Func<int> maxAttempts, Func<float> attemptIntervalSec, Func<float> cooldownSec)
        {
            _maxAttempts = maxAttempts;
            _attemptIntervalSec = attemptIntervalSec;
            _cooldownSec = cooldownSec;
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
