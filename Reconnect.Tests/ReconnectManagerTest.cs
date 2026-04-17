using Reconnect.Core;

namespace Reconnect.Tests;

public class ReconnectManagerTest
{
    private ReconnectManager _mgr = null!;

    [SetUp]
    public void Setup()
    {
        _mgr = new ReconnectManager();
    }

    // -- Constructor / clamping --------------------------------------------

    [Test]
    public void Ctor_DefaultValues()
    {
        Assert.That(_mgr.MaxAttempts, Is.EqualTo(3));
        Assert.That(_mgr.AttemptIntervalSec, Is.EqualTo(5f));
        Assert.That(_mgr.CooldownSec, Is.EqualTo(30f));
    }

    [Test]
    public void Ctor_ClampsMaxAttempts_Low()
    {
        var mgr = new ReconnectManager(maxAttempts: 0);
        Assert.That(mgr.MaxAttempts, Is.EqualTo(1));
    }

    [Test]
    public void Ctor_ClampsMaxAttempts_High()
    {
        var mgr = new ReconnectManager(maxAttempts: 99);
        Assert.That(mgr.MaxAttempts, Is.EqualTo(10));
    }

    [Test]
    public void Ctor_ClampsAttemptInterval_Low()
    {
        var mgr = new ReconnectManager(attemptIntervalSec: 0.5f);
        Assert.That(mgr.AttemptIntervalSec, Is.EqualTo(2f));
    }

    [Test]
    public void Ctor_ClampsAttemptInterval_High()
    {
        var mgr = new ReconnectManager(attemptIntervalSec: 100f);
        Assert.That(mgr.AttemptIntervalSec, Is.EqualTo(30f));
    }

    [Test]
    public void Ctor_ClampsCooldown_Low()
    {
        var mgr = new ReconnectManager(cooldownSec: 1f);
        Assert.That(mgr.CooldownSec, Is.EqualTo(10f));
    }

    [Test]
    public void Ctor_ClampsCooldown_High()
    {
        var mgr = new ReconnectManager(cooldownSec: 999f);
        Assert.That(mgr.CooldownSec, Is.EqualTo(120f));
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
        var mgr = new ReconnectManager(maxAttempts: 5);
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
