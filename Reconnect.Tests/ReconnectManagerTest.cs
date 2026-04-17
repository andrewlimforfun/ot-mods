using Reconnect.Core;

namespace Reconnect.Tests;

public class ReconnectManagerTest
{
    private int _maxAttempts;
    private float _intervalSec;
    private float _cooldownSec;
    private ReconnectManager _mgr = null!;

    [SetUp]
    public void Setup()
    {
        _maxAttempts = 3;
        _intervalSec = 5f;
        _cooldownSec = 30f;
        _mgr = new ReconnectManager(() => _maxAttempts, () => _intervalSec, () => _cooldownSec);
    }

    // -- Live config reads -------------------------------------------------

    [Test]
    public void MaxAttempts_ReflectsLiveChange()
    {
        _maxAttempts = 3;
        Assert.That(_mgr.MaxAttempts, Is.EqualTo(3));

        _maxAttempts = 7;
        Assert.That(_mgr.MaxAttempts, Is.EqualTo(7));
    }

    [Test]
    public void AttemptIntervalSec_ReflectsLiveChange()
    {
        _intervalSec = 5f;
        Assert.That(_mgr.AttemptIntervalSec, Is.EqualTo(5f));

        _intervalSec = 10f;
        Assert.That(_mgr.AttemptIntervalSec, Is.EqualTo(10f));
    }

    [Test]
    public void CooldownSec_ReflectsLiveChange()
    {
        _cooldownSec = 30f;
        Assert.That(_mgr.CooldownSec, Is.EqualTo(30f));

        _cooldownSec = 60f;
        Assert.That(_mgr.CooldownSec, Is.EqualTo(60f));
    }

    // -- TryBeginSequence --------------------------------------------------

    [Test]
    public void TryBeginSequence_FirstCall_ReturnsTrue()
    {
        Assert.That(_mgr.TryBeginSequence(100f), Is.True);
        Assert.That(_mgr.IsReconnecting, Is.True);
    }

    [Test]
    public void TryBeginSequence_WithinCooldown_ReturnsFalse()
    {
        _mgr.TryBeginSequence(100f);
        _mgr.OnFailed(); // end the sequence

        Assert.That(_mgr.TryBeginSequence(110f), Is.False);
        Assert.That(_mgr.IsReconnecting, Is.False);
    }

    [Test]
    public void TryBeginSequence_AfterCooldown_ReturnsTrue()
    {
        _mgr.TryBeginSequence(100f);
        _mgr.OnFailed();

        Assert.That(_mgr.TryBeginSequence(100f + _mgr.CooldownSec + 1f), Is.True);
        Assert.That(_mgr.IsReconnecting, Is.True);
    }

    [Test]
    public void TryBeginSequence_ResetsAttemptCounter()
    {
        _mgr.TryBeginSequence(100f);
        _mgr.TryNextAttempt();
        _mgr.TryNextAttempt();
        _mgr.OnFailed();

        _mgr.TryBeginSequence(100f + _mgr.CooldownSec + 1f);
        Assert.That(_mgr.CurrentAttempt, Is.EqualTo(0));
    }

    // -- TryNextAttempt ----------------------------------------------------

    [Test]
    public void TryNextAttempt_IncrementsCurrentAttempt()
    {
        _mgr.TryBeginSequence(100f);

        Assert.That(_mgr.TryNextAttempt(), Is.True);
        Assert.That(_mgr.CurrentAttempt, Is.EqualTo(1));

        Assert.That(_mgr.TryNextAttempt(), Is.True);
        Assert.That(_mgr.CurrentAttempt, Is.EqualTo(2));
    }

    [Test]
    public void TryNextAttempt_ExhaustsAtMaxAttempts()
    {
        _mgr.TryBeginSequence(100f);
        for (int i = 0; i < _mgr.MaxAttempts; i++)
            _mgr.TryNextAttempt();

        Assert.That(_mgr.TryNextAttempt(), Is.False);
        Assert.That(_mgr.IsReconnecting, Is.False);
    }

    [Test]
    public void TryNextAttempt_ReturnsFalse_WhenIntentionalLeave()
    {
        _mgr.TryBeginSequence(100f);
        _mgr.IsIntentionalLeave = true;

        Assert.That(_mgr.TryNextAttempt(), Is.False);
        Assert.That(_mgr.IsReconnecting, Is.False);
    }

    [Test]
    public void TryNextAttempt_CustomMaxAttempts()
    {
        var mgr = new ReconnectManager(() => 5, () => 5f, () => 30f);
        mgr.TryBeginSequence(100f);

        int count = 0;
        while (mgr.TryNextAttempt()) count++;

        Assert.That(count, Is.EqualTo(5));
    }

    // -- OnConnected -------------------------------------------------------

    [Test]
    public void OnConnected_ClearsFlags()
    {
        _mgr.TryBeginSequence(100f);
        _mgr.IsIntentionalLeave = true;

        _mgr.OnConnected();

        Assert.That(_mgr.IsReconnecting, Is.False);
        Assert.That(_mgr.IsIntentionalLeave, Is.False);
    }

    // -- OnFailed ----------------------------------------------------------

    [Test]
    public void OnFailed_ClearsIsReconnecting()
    {
        _mgr.TryBeginSequence(100f);

        _mgr.OnFailed();

        Assert.That(_mgr.IsReconnecting, Is.False);
    }

    // -- Full sequences ----------------------------------------------------

    [Test]
    public void FullSequence_AllAttemptsExhausted()
    {
        _mgr.TryBeginSequence(100f);

        int attempts = 0;
        while (_mgr.TryNextAttempt())
            attempts++;

        Assert.That(attempts, Is.EqualTo(_mgr.MaxAttempts));
        Assert.That(_mgr.IsReconnecting, Is.False);
    }

    [Test]
    public void FullSequence_ConnectedMidway()
    {
        _mgr.TryBeginSequence(100f);
        _mgr.TryNextAttempt(); // attempt 1

        _mgr.OnConnected();

        Assert.That(_mgr.IsReconnecting, Is.False);
        Assert.That(_mgr.IsIntentionalLeave, Is.False);
        Assert.That(_mgr.CurrentAttempt, Is.EqualTo(1));
    }

    [Test]
    public void FullSequence_IntentionalLeaveMidway()
    {
        _mgr.TryBeginSequence(100f);
        _mgr.TryNextAttempt(); // attempt 1

        _mgr.IsIntentionalLeave = true;

        Assert.That(_mgr.TryNextAttempt(), Is.False);
        Assert.That(_mgr.IsReconnecting, Is.False);
    }

    [Test]
    public void FullSequence_CooldownPreventsRapidLoop()
    {
        // First sequence
        _mgr.TryBeginSequence(100f);
        while (_mgr.TryNextAttempt()) { }

        // Immediate second sequence - blocked
        Assert.That(_mgr.TryBeginSequence(105f), Is.False);

        // After cooldown - allowed
        Assert.That(_mgr.TryBeginSequence(100f + _mgr.CooldownSec + 1f), Is.True);
    }

    // -- SavedLobbyId ------------------------------------------------------

    [Test]
    public void SavedLobbyId_GetSet()
    {
        Assert.That(_mgr.SavedLobbyId, Is.Null);
        _mgr.SavedLobbyId = "ABC123";
        Assert.That(_mgr.SavedLobbyId, Is.EqualTo("ABC123"));
    }
}
