using Hush.Core;

namespace Hush.Tests;

public class PlayerMuteManagerTest
{
    private PlayerMuteManager _mutes = null!;

    [SetUp]
    public void Setup()
    {
        _mutes = new PlayerMuteManager();
    }

    // -- Permanent mute ----------------------------------------------------

    [Test]
    public void Mute_NewPlayer_ReturnsTrue()
    {
        Assert.That(_mutes.Mute("steam_001"), Is.True);
    }

    [Test]
    public void Mute_AlreadyMuted_ReturnsFalse()
    {
        _mutes.Mute("steam_001");
        Assert.That(_mutes.Mute("steam_001"), Is.False);
    }

    [Test]
    public void Mute_OverridesTimedMute()
    {
        _mutes.MuteFor("steam_001", TimeSpan.FromMinutes(5));
        Assert.That(_mutes.Mute("steam_001"), Is.True);
        Assert.That(_mutes.IsMuted("steam_001"), Is.True);

        // Should now be permanent - verify via GetMutes
        var list = _mutes.GetMutes();
        var entry = list.Find(m => m.SteamId == "steam_001");
        Assert.That(entry.Expiry, Is.Null, "Should be permanent (null expiry)");
    }

    // -- Timed mute --------------------------------------------------------

    [Test]
    public void MuteFor_ValidDuration_ReturnsTrue()
    {
        Assert.That(_mutes.MuteFor("steam_001", TimeSpan.FromMinutes(5)), Is.True);
    }

    [Test]
    public void MuteFor_ZeroDuration_ReturnsFalse()
    {
        Assert.That(_mutes.MuteFor("steam_001", TimeSpan.Zero), Is.False);
    }

    [Test]
    public void MuteFor_NegativeDuration_ReturnsFalse()
    {
        Assert.That(_mutes.MuteFor("steam_001", TimeSpan.FromSeconds(-10)), Is.False);
    }

    [Test]
    public void MuteFor_OverridesPermanentMute()
    {
        _mutes.Mute("steam_001");
        Assert.That(_mutes.MuteFor("steam_001", TimeSpan.FromMinutes(5)), Is.True);

        var list = _mutes.GetMutes();
        var entry = list.Find(m => m.SteamId == "steam_001");
        Assert.That(entry.Expiry, Is.Not.Null, "Should be timed (non-null expiry)");
    }

    [Test]
    public void MuteFor_ReplacesExistingTimedMute()
    {
        _mutes.MuteFor("steam_001", TimeSpan.FromSeconds(30));
        Assert.That(_mutes.MuteFor("steam_001", TimeSpan.FromMinutes(10)), Is.True);

        var list = _mutes.GetMutes();
        var entry = list.Find(m => m.SteamId == "steam_001");
        Assert.That(entry.Expiry, Is.Not.Null);
        // New expiry should be ~10 minutes from now, not ~30 seconds
        double remaining = (entry.Expiry!.Value - DateTime.UtcNow).TotalMinutes;
        Assert.That(remaining, Is.GreaterThan(5));
    }

    // -- IsMuted -----------------------------------------------------------

    [Test]
    public void IsMuted_PermanentlyMuted_ReturnsTrue()
    {
        _mutes.Mute("steam_001");
        Assert.That(_mutes.IsMuted("steam_001"), Is.True);
    }

    [Test]
    public void IsMuted_TimedAndActive_ReturnsTrue()
    {
        _mutes.MuteFor("steam_001", TimeSpan.FromMinutes(5));
        Assert.That(_mutes.IsMuted("steam_001"), Is.True);
    }

    [Test]
    public void IsMuted_NotMuted_ReturnsFalse()
    {
        Assert.That(_mutes.IsMuted("steam_999"), Is.False);
    }

    // -- Unmute ------------------------------------------------------------

    [Test]
    public void Unmute_PermanentlyMuted_ReturnsTrue()
    {
        _mutes.Mute("steam_001");
        Assert.That(_mutes.Unmute("steam_001"), Is.True);
        Assert.That(_mutes.IsMuted("steam_001"), Is.False);
    }

    [Test]
    public void Unmute_TimedMuted_ReturnsTrue()
    {
        _mutes.MuteFor("steam_001", TimeSpan.FromMinutes(5));
        Assert.That(_mutes.Unmute("steam_001"), Is.True);
        Assert.That(_mutes.IsMuted("steam_001"), Is.False);
    }

    [Test]
    public void Unmute_NotMuted_ReturnsFalse()
    {
        Assert.That(_mutes.Unmute("steam_999"), Is.False);
    }

    // -- Delegates ---------------------------------------------------------

    [Test]
    public void AddDelegate_New_ReturnsTrue()
    {
        Assert.That(_mutes.AddDelegate("steam_d01"), Is.True);
    }

    [Test]
    public void AddDelegate_Duplicate_ReturnsFalse()
    {
        _mutes.AddDelegate("steam_d01");
        Assert.That(_mutes.AddDelegate("steam_d01"), Is.False);
    }

    [Test]
    public void IsDelegate_Added_ReturnsTrue()
    {
        _mutes.AddDelegate("steam_d01");
        Assert.That(_mutes.IsDelegate("steam_d01"), Is.True);
    }

    [Test]
    public void IsDelegate_NotAdded_ReturnsFalse()
    {
        Assert.That(_mutes.IsDelegate("steam_d01"), Is.False);
    }

    [Test]
    public void RemoveDelegate_Existing_ReturnsTrue()
    {
        _mutes.AddDelegate("steam_d01");
        Assert.That(_mutes.RemoveDelegate("steam_d01"), Is.True);
        Assert.That(_mutes.IsDelegate("steam_d01"), Is.False);
    }

    [Test]
    public void RemoveDelegate_Missing_ReturnsFalse()
    {
        Assert.That(_mutes.RemoveDelegate("steam_d01"), Is.False);
    }

    // -- GetMutes ----------------------------------------------------------

    [Test]
    public void GetMutes_Empty_ReturnsEmpty()
    {
        Assert.That(_mutes.GetMutes(), Is.Empty);
    }

    [Test]
    public void GetMutes_MixedMutes_ReturnsAll()
    {
        _mutes.Mute("steam_p1");
        _mutes.MuteFor("steam_t1", TimeSpan.FromMinutes(5));
        var list = _mutes.GetMutes();
        Assert.That(list, Has.Count.EqualTo(2));
    }

    // -- Save / Load -------------------------------------------------------

    [Test]
    public void SaveAndLoad_RoundTrips()
    {
        _mutes.Mute("steam_perma");
        _mutes.MuteFor("steam_timed", TimeSpan.FromMinutes(30));
        _mutes.AddDelegate("steam_del");

        string path = Path.Combine(Path.GetTempPath(), $"hush_mute_test_{Guid.NewGuid()}.json");
        try
        {
            _mutes.Save(path);

            var loaded = new PlayerMuteManager();
            loaded.Load(path);

            Assert.That(loaded.IsMuted("steam_perma"), Is.True);
            Assert.That(loaded.IsMuted("steam_timed"), Is.True);
            Assert.That(loaded.IsDelegate("steam_del"), Is.True);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Load_MissingFile_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _mutes.Load(@"C:\nonexistent\path\mutes.json"));
    }

    [Test]
    public void Load_ExpiredTimedMutes_AreNotRestored()
    {
        // Craft a JSON with an already-expired timed mute
        string expired = DateTime.UtcNow.AddMinutes(-5).ToString("o");
        string json = $$"""
        {
            "PermaMuted": [],
            "TimedMutes": { "steam_expired": "{{expired}}" },
            "Delegates": []
        }
        """;
        string path = Path.Combine(Path.GetTempPath(), $"hush_mute_test_{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(path, json);
            _mutes.Load(path);
            Assert.That(_mutes.IsMuted("steam_expired"), Is.False);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // -- Multiple players --------------------------------------------------

    [Test]
    public void MultiplePlayersIndependent()
    {
        _mutes.Mute("steam_001");
        _mutes.MuteFor("steam_002", TimeSpan.FromMinutes(5));

        Assert.That(_mutes.IsMuted("steam_001"), Is.True);
        Assert.That(_mutes.IsMuted("steam_002"), Is.True);
        Assert.That(_mutes.IsMuted("steam_003"), Is.False);

        _mutes.Unmute("steam_001");
        Assert.That(_mutes.IsMuted("steam_001"), Is.False);
        Assert.That(_mutes.IsMuted("steam_002"), Is.True);
    }
}
