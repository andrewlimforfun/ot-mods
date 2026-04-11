using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Hush.Core;

namespace Hush.Tests;

public class RelayExecutorTest
{
    private PlayerMuteManager _mutes = null!;
    private List<(string SteamId, string Name)> _banned = null!;
    private List<string> _notifications = null!;
    private bool _mutesSaved;
    private Dictionary<string, string> _steamToName = null!;  // steamId → display name
    private Dictionary<string, string> _queryToSteam = null!; // query → steamId
    private RelayExecutor _executor = null!;

    private const string SenderSteamId = "76561198000000001";
    private const string TargetSteamId = "76561198000000002";

    [SetUp]
    public void Setup()
    {
        _mutes = new PlayerMuteManager();
        _banned = new List<(string, string)>();
        _notifications = new List<string>();
        _mutesSaved = false;
        _steamToName = new Dictionary<string, string>
        {
            [SenderSteamId] = "Alice",
            [TargetSteamId] = "Bob",
        };
        _queryToSteam = new Dictionary<string, string>();
        _executor = MakeExecutor();
    }

    private RelayExecutor MakeExecutor(Func<string, string, bool>? ban = null) => new RelayExecutor(
        mutes: _mutes,
        ban: ban ?? ((id, name) => { _banned.Add((id, name)); return true; }),
        unmute: id => _mutes.Unmute(id),
        resolveName: id => _steamToName.TryGetValue(id, out var n) ? n : null,
        resolveQuery: q => _queryToSteam.TryGetValue(q, out var id) ? id : null,
        notify: msg => _notifications.Add(msg),
        saveMutes: () => _mutesSaved = true,
        log: Logger.CreateLogSource("RelayExecutorTest")
    );

    // -- invalid payloads --------------------------------------------------

    [Test]
    public void Execute_EmptyPayload_ReturnsFalse()
    {
        Assert.That(_executor.Execute("", SenderSteamId), Is.False);
    }

    [Test]
    public void Execute_MissingPrefix_ReturnsFalse()
    {
        Assert.That(_executor.Execute("tmute:steam_001:60", SenderSteamId), Is.False);
    }

    [Test]
    public void Execute_MalformedPayload_ReturnsFalse()
    {
        Assert.That(_executor.Execute("hush:notacommand", SenderSteamId), Is.False);
    }

    [Test]
    public void Execute_UnknownCommand_ReturnsFalse()
    {
        Assert.That(_executor.Execute($"hush:kick:{TargetSteamId}", SenderSteamId), Is.False);
    }

    // -- tmute: by Steam ID ------------------------------------------------

    [Test]
    public void Execute_Tmute_BySteamId_ReturnsTrue()
    {
        Assert.That(_executor.Execute($"hush:tmute:{TargetSteamId}:300", SenderSteamId), Is.True);
    }

    [Test]
    public void Execute_Tmute_BySteamId_MutesPlayer()
    {
        _executor.Execute($"hush:tmute:{TargetSteamId}:300", SenderSteamId);
        Assert.That(_mutes.IsMuted(TargetSteamId), Is.True);
    }

    [Test]
    public void Execute_Tmute_BySteamId_SavesMutes()
    {
        _executor.Execute($"hush:tmute:{TargetSteamId}:300", SenderSteamId);
        Assert.That(_mutesSaved, Is.True);
    }

    [Test]
    public void Execute_Tmute_BySteamId_Notifies_WithTargetAndSender()
    {
        _executor.Execute($"hush:tmute:{TargetSteamId}:300", SenderSteamId);
        Assert.That(_notifications, Has.Count.EqualTo(1));
        Assert.That(_notifications[0], Does.Contain("Bob").And.Contain("Alice").And.Contain(TargetSteamId));
    }

    [Test]
    public void Execute_Tmute_BySteamId_OfflinePlayer_StillMutes()
    {
        // Player not in name lookup (offline) but Steam ID64 is valid — must still mute
        const string offlineId = "76561198000000099";
        _executor.Execute($"hush:tmute:{offlineId}:60", SenderSteamId);
        Assert.That(_mutes.IsMuted(offlineId), Is.True);
    }

    [Test]
    public void Execute_Tmute_BySteamId_OfflinePlayer_FallsBackToIdInNotification()
    {
        const string offlineId = "76561198000000099";
        _executor.Execute($"hush:tmute:{offlineId}:60", SenderSteamId);
        Assert.That(_notifications[0], Does.Contain(offlineId));
    }

    [Test]
    public void Execute_Tmute_ZeroDuration_ReturnsFalse()
    {
        Assert.That(_executor.Execute($"hush:tmute:{TargetSteamId}:0", SenderSteamId), Is.False);
    }

    [Test]
    public void Execute_Tmute_ZeroDuration_DoesNotMute()
    {
        _executor.Execute($"hush:tmute:{TargetSteamId}:0", SenderSteamId);
        Assert.That(_mutes.IsMuted(TargetSteamId), Is.False);
    }

    // -- tmute: by query ---------------------------------------------------

    [Test]
    public void Execute_Tmute_ByQuery_ResolvesAndMutes()
    {
        _queryToSteam["bob"] = TargetSteamId;
        _executor.Execute("hush:tmute:bob:300", SenderSteamId);
        Assert.That(_mutes.IsMuted(TargetSteamId), Is.True);
    }

    [Test]
    public void Execute_Tmute_ByQuery_SavesMutes()
    {
        _queryToSteam["bob"] = TargetSteamId;
        _executor.Execute("hush:tmute:bob:300", SenderSteamId);
        Assert.That(_mutesSaved, Is.True);
    }

    [Test]
    public void Execute_Tmute_ByQuery_NotFound_ReturnsFalse()
    {
        Assert.That(_executor.Execute("hush:tmute:unknownplayer:300", SenderSteamId), Is.False);
    }

    [Test]
    public void Execute_Tmute_ByQuery_NotFound_DoesNotMute()
    {
        _executor.Execute("hush:tmute:unknownplayer:300", SenderSteamId);
        Assert.That(_mutes.GetMutes(), Is.Empty);
    }

    [Test]
    public void Execute_Tmute_ByQuery_NotFound_DoesNotSave()
    {
        _executor.Execute("hush:tmute:unknownplayer:300", SenderSteamId);
        Assert.That(_mutesSaved, Is.False);
    }

    [Test]
    public void Execute_Tmute_ByQuery_Notifies_WithResolvedId()
    {
        _queryToSteam["bob"] = TargetSteamId;
        _executor.Execute("hush:tmute:bob:60", SenderSteamId);
        Assert.That(_notifications, Has.Count.EqualTo(1));
        Assert.That(_notifications[0], Does.Contain(TargetSteamId));
    }

    // -- ban: by Steam ID --------------------------------------------------

    [Test]
    public void Execute_Ban_BySteamId_ReturnsTrue()
    {
        Assert.That(_executor.Execute($"hush:ban:{TargetSteamId}", SenderSteamId), Is.True);
    }

    [Test]
    public void Execute_Ban_BySteamId_BansPlayer()
    {
        _executor.Execute($"hush:ban:{TargetSteamId}", SenderSteamId);
        Assert.That(_banned, Has.Count.EqualTo(1));
        Assert.That(_banned[0].SteamId, Is.EqualTo(TargetSteamId));
    }

    [Test]
    public void Execute_Ban_BySteamId_PassesDisplayNameToBan()
    {
        _executor.Execute($"hush:ban:{TargetSteamId}", SenderSteamId);
        Assert.That(_banned[0].Name, Is.EqualTo("Bob"));
    }

    [Test]
    public void Execute_Ban_BySteamId_Notifies()
    {
        _executor.Execute($"hush:ban:{TargetSteamId}", SenderSteamId);
        Assert.That(_notifications, Has.Count.EqualTo(1));
        Assert.That(_notifications[0], Does.Contain("Bob").And.Contain(TargetSteamId));
    }

    [Test]
    public void Execute_Ban_AlreadyBanned_NoNotification()
    {
        var executor = MakeExecutor(ban: (_, _) => false);
        executor.Execute($"hush:ban:{TargetSteamId}", SenderSteamId);
        Assert.That(_notifications, Is.Empty);
    }

    [Test]
    public void Execute_Ban_BySteamId_OfflinePlayer_StillBans()
    {
        const string offlineId = "76561198000000099";
        _executor.Execute($"hush:ban:{offlineId}", SenderSteamId);
        Assert.That(_banned, Has.Count.EqualTo(1));
        Assert.That(_banned[0].SteamId, Is.EqualTo(offlineId));
    }

    // -- ban: by query -----------------------------------------------------

    [Test]
    public void Execute_Ban_ByQuery_ResolvesAndBans()
    {
        _queryToSteam["bob"] = TargetSteamId;
        _executor.Execute("hush:ban:bob", SenderSteamId);
        Assert.That(_banned, Has.Count.EqualTo(1));
        Assert.That(_banned[0].SteamId, Is.EqualTo(TargetSteamId));
    }

    [Test]
    public void Execute_Ban_ByQuery_NotFound_ReturnsFalse()
    {
        Assert.That(_executor.Execute("hush:ban:unknownplayer", SenderSteamId), Is.False);
    }

    [Test]
    public void Execute_Ban_ByQuery_NotFound_NoBan()
    {
        _executor.Execute("hush:ban:unknownplayer", SenderSteamId);
        Assert.That(_banned, Is.Empty);
    }

    // -- unmute: by Steam ID -----------------------------------------------

    [Test]
    public void Execute_Unmute_BySteamId_ReturnsTrue()
    {
        _mutes.Mute(TargetSteamId);
        Assert.That(_executor.Execute($"hush:unmute:{TargetSteamId}", SenderSteamId), Is.True);
    }

    [Test]
    public void Execute_Unmute_BySteamId_UnmutesPlayer()
    {
        _mutes.Mute(TargetSteamId);
        _executor.Execute($"hush:unmute:{TargetSteamId}", SenderSteamId);
        Assert.That(_mutes.IsMuted(TargetSteamId), Is.False);
    }

    [Test]
    public void Execute_Unmute_BySteamId_SavesMutes()
    {
        _mutes.Mute(TargetSteamId);
        _executor.Execute($"hush:unmute:{TargetSteamId}", SenderSteamId);
        Assert.That(_mutesSaved, Is.True);
    }

    [Test]
    public void Execute_Unmute_BySteamId_Notifies()
    {
        _mutes.Mute(TargetSteamId);
        _executor.Execute($"hush:unmute:{TargetSteamId}", SenderSteamId);
        Assert.That(_notifications, Has.Count.EqualTo(1));
        Assert.That(_notifications[0], Does.Contain("Bob").And.Contain(TargetSteamId));
    }

    [Test]
    public void Execute_Unmute_NotMuted_ReturnsTrueNoNotification()
    {
        // Player is not muted — unmute is a no-op but the relay itself succeeded
        _executor.Execute($"hush:unmute:{TargetSteamId}", SenderSteamId);
        Assert.That(_notifications, Is.Empty);
    }

    [Test]
    public void Execute_Unmute_NotMuted_DoesNotSave()
    {
        _executor.Execute($"hush:unmute:{TargetSteamId}", SenderSteamId);
        Assert.That(_mutesSaved, Is.False);
    }

    // -- unmute: by query --------------------------------------------------

    [Test]
    public void Execute_Unmute_ByQuery_ResolvesAndUnmutes()
    {
        _mutes.Mute(TargetSteamId);
        _queryToSteam["bob"] = TargetSteamId;
        _executor.Execute("hush:unmute:bob", SenderSteamId);
        Assert.That(_mutes.IsMuted(TargetSteamId), Is.False);
    }

    [Test]
    public void Execute_Unmute_ByQuery_NotFound_ReturnsFalse()
    {
        Assert.That(_executor.Execute("hush:unmute:unknownplayer", SenderSteamId), Is.False);
    }
}
