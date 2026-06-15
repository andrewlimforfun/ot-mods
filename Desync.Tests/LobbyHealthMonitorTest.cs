using Desync.Core;

namespace Desync.Tests;

public class LobbyHealthMonitorTest
{
    // --------------------- Healthy State ---------------------

    [Test]
    public void Healthy_AllGood_IsHealthyTrue()
    {
        var diag = MakeDiag();
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.IsHealthy, Is.True);
        Assert.That(monitor.Summary, Is.EqualTo(""));
    }

    // --------------------- H1: Lobby Metadata ---------------------

    [Test]
    public void H1_LobbyNotValid_Detected()
    {
        var diag = MakeDiag(lobbyValid: false);
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasLobbyMetadataIssue, Is.True);
        Assert.That(monitor.IsHealthy, Is.False);
        Assert.That(monitor.Summary, Does.Contain("H1"));
    }

    [Test]
    public void H1_LobbyOwnerZero_Detected()
    {
        var diag = MakeDiag(lobbyOwnerSteamId: "0");
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasLobbyMetadataIssue, Is.True);
    }

    [Test]
    public void H1_LobbyOwnerEmpty_Detected()
    {
        var diag = MakeDiag(lobbyOwnerSteamId: "");
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasLobbyMetadataIssue, Is.True);
    }

    // --------------------- H2: Host Identity ---------------------

    [Test]
    public void H2_LobbyOwnerNotHost_Detected()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            localSteamId: "76561198012345678",
            isHost: false);
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasHostIdentityIssue, Is.True);
        Assert.That(monitor.Summary, Does.Contain("H2"));
    }

    [Test]
    public void H2_LobbyOwnerIsHost_NotDetected()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            localSteamId: "76561198012345678",
            isHost: true);
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasHostIdentityIssue, Is.False);
    }

    [Test]
    public void H2_NotLobbyOwner_NotDetected()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            localSteamId: "76561198099999999",
            isHost: false);
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasHostIdentityIssue, Is.False);
    }

    // --------------------- H3: Player List ---------------------

    [Test]
    public void H3_HostMissingFromList_Detected()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198099999999" });
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasPlayerListIssue, Is.True);
        Assert.That(monitor.Summary, Does.Contain("H3"));
    }

    [Test]
    public void H3_HostInList_NotDetected()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198012345678" });
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasPlayerListIssue, Is.False);
    }

    // --------------------- H4: Persona Cache ---------------------

    [Test]
    public void H4_EmptyPersona_Detected()
    {
        var diag = MakeDiag(
            lobbyOwnerSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198012345678" },
            hostPersonaName: "");
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasPersonaCacheIssue, Is.True);
        Assert.That(monitor.Summary, Does.Contain("H4"));
    }

    [Test]
    public void H4_ValidPersona_NotDetected()
    {
        var diag = MakeDiag(hostPersonaName: "SomePlayer");
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasPersonaCacheIssue, Is.False);
    }

    // --------------------- Compound Issues ---------------------

    [Test]
    public void Compound_H1AndH2_BothDetected()
    {
        var diag = MakeDiag(
            lobbyValid: false,
            lobbyOwnerSteamId: "76561198012345678",
            localSteamId: "76561198012345678",
            isHost: false);
        var monitor = new LobbyHealthMonitor(diag);
        Assert.That(monitor.HasLobbyMetadataIssue, Is.True);
        Assert.That(monitor.HasHostIdentityIssue, Is.True);
        Assert.That(monitor.Summary, Does.Contain("H1"));
        Assert.That(monitor.Summary, Does.Contain("H2"));
    }

    [Test]
    public void Compound_AllIssues_AllDetected()
    {
        var diag = MakeDiag(
            lobbyValid: false,
            lobbyOwnerSteamId: "76561198012345678",
            localSteamId: "76561198012345678",
            playerSteamIds: new() { "76561198099999999" },
            isHost: false,
            hostPersonaName: "");
        var monitor = new LobbyHealthMonitor(diag);
        // H1 (invalid lobby), H2 won't trigger because IsLobbyOwnerInvalid is false but lobbyValid is separate
        // Actually H2 checks IsLobbyOwnerInvalid which is based on the steam ID, not lobbyValid
        Assert.That(monitor.HasLobbyMetadataIssue, Is.True);
        Assert.That(monitor.HasHostIdentityIssue, Is.True);
        Assert.That(monitor.HasPlayerListIssue, Is.True);
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
