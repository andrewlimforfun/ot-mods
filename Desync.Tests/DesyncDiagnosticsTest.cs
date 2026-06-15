using Desync.Core;

namespace Desync.Tests;

public class DesyncDiagnosticsTest
{
    // --------------------- IsLobbyOwnerInvalid ---------------------

    [Test]
    public void IsLobbyOwnerInvalid_EmptyString_True()
    {
        var diag = MakeDiag(lobbyOwnerSteamId: "");
        Assert.That(diag.IsLobbyOwnerInvalid, Is.True);
    }

    [Test]
    public void IsLobbyOwnerInvalid_Zero_True()
    {
        var diag = MakeDiag(lobbyOwnerSteamId: "0");
        Assert.That(diag.IsLobbyOwnerInvalid, Is.True);
    }

    [Test]
    public void IsLobbyOwnerInvalid_ValidId_False()
    {
        var diag = MakeDiag(lobbyOwnerSteamId: "76561198012345678");
        Assert.That(diag.IsLobbyOwnerInvalid, Is.False);
    }

    // --------------------- IsServerIdentityLost ---------------------

    [Test]
    public void IsServerIdentityLost_LobbyOwnerButNotHost_True()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            localSteamId: "76561198012345678",
            isHost: false);
        Assert.That(diag.IsServerIdentityLost, Is.True);
    }

    [Test]
    public void IsServerIdentityLost_LobbyOwnerAndIsHost_False()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            localSteamId: "76561198012345678",
            isHost: true);
        Assert.That(diag.IsServerIdentityLost, Is.False);
    }

    [Test]
    public void IsServerIdentityLost_NotLobbyOwner_False()
    {
        // Regular client - isHost false is expected
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            localSteamId: "76561198099999999",
            isHost: false);
        Assert.That(diag.IsServerIdentityLost, Is.False);
    }

    [Test]
    public void IsServerIdentityLost_InvalidLobbyOwner_False()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "0",
            localSteamId: "0",
            isHost: false);
        Assert.That(diag.IsServerIdentityLost, Is.False);
    }

    // --------------------- IsHostMissingFromList ---------------------

    [Test]
    public void IsHostMissingFromList_OwnerNotInList_True()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198099999999", "76561198088888888" });
        Assert.That(diag.IsHostMissingFromList, Is.True);
    }

    [Test]
    public void IsHostMissingFromList_OwnerInList_False()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198012345678", "76561198088888888" });
        Assert.That(diag.IsHostMissingFromList, Is.False);
    }

    [Test]
    public void IsHostMissingFromList_InvalidOwner_False()
    {
        // If owner is invalid, we can't determine if they're missing
        var diag = MakeDiag(
            lobbyOwnerSteamId: "0",
            playerSteamIds: new() { "76561198099999999" });
        Assert.That(diag.IsHostMissingFromList, Is.False);
    }

    // --------------------- IsPersonaCacheStale ---------------------

    [Test]
    public void IsPersonaCacheStale_EmptyPersonaName_True()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198012345678" },
            hostPersonaName: "");
        Assert.That(diag.IsPersonaCacheStale, Is.True);
    }

    [Test]
    public void IsPersonaCacheStale_ValidPersonaName_False()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198012345678" },
            hostPersonaName: "PlayerName");
        Assert.That(diag.IsPersonaCacheStale, Is.False);
    }

    [Test]
    public void IsPersonaCacheStale_HostNotInList_False()
    {
        // H3 takes precedence - can't be persona cache if host isn't even in list
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198099999999" },
            hostPersonaName: "");
        Assert.That(diag.IsPersonaCacheStale, Is.False);
    }

    // --------------------- Helper ---------------------

    static DesyncDiagnostics MakeDiag(
        bool lobbyValid = true,
        string lobbyOwnerSteamId = "76561198012345678",
        string localSteamId = "76561198099999999",
        List<string>? playerSteamIds = null,
        int totalTrackedPlayers = 5,
        string hostPersonaName = "TestHost",
        string hostUserName = "TestHost",
        bool isHost = false)
    {
        return new DesyncDiagnostics(
            lobbyValid,
            lobbyOwnerSteamId,
            localSteamId,
            playerSteamIds ?? new List<string> { "76561198012345678", "76561198099999999" },
            totalTrackedPlayers,
            hostPersonaName,
            hostUserName,
            isHost);
    }
}
