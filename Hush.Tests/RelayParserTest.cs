using Hush.Core;

namespace Hush.Tests;

public class RelayParserTest
{
    // -- tmute -------------------------------------------------------------

    [Test]
    public void Parse_Tmute_ValidPayload()
    {
        var cmd = RelayParser.Parse("tmute:steam_001:60");
        Assert.That(cmd.IsValid, Is.True);
        Assert.That(cmd.Type, Is.EqualTo(RelayCommandType.TimedMute));
        Assert.That(cmd.TargetSteamId, Is.EqualTo("steam_001"));
        Assert.That(cmd.DurationSeconds, Is.EqualTo(60));
    }

    [Test]
    public void Parse_Tmute_LargeDuration()
    {
        var cmd = RelayParser.Parse("tmute:steam_001:3600");
        Assert.That(cmd.IsValid, Is.True);
        Assert.That(cmd.DurationSeconds, Is.EqualTo(3600));
    }

    [Test]
    public void Parse_Tmute_SteamIdWithColons()
    {
        // Steam IDs don't normally have colons, but the parser uses LastIndexOf
        // to split, so a colon in the target would still parse if duration is last segment
        var cmd = RelayParser.Parse("tmute:some:weird:id:120");
        Assert.That(cmd.IsValid, Is.True);
        Assert.That(cmd.TargetSteamId, Is.EqualTo("some:weird:id"));
        Assert.That(cmd.DurationSeconds, Is.EqualTo(120));
    }

    [Test]
    public void Parse_Tmute_MissingDuration()
    {
        var cmd = RelayParser.Parse("tmute:steam_001");
        Assert.That(cmd.IsValid, Is.False);
        Assert.That(cmd.Error, Does.Contain("tmute"));
    }

    [Test]
    public void Parse_Tmute_ZeroDuration()
    {
        var cmd = RelayParser.Parse("tmute:steam_001:0");
        Assert.That(cmd.IsValid, Is.False);
    }

    [Test]
    public void Parse_Tmute_NegativeDuration()
    {
        var cmd = RelayParser.Parse("tmute:steam_001:-10");
        Assert.That(cmd.IsValid, Is.False);
    }

    [Test]
    public void Parse_Tmute_NonNumericDuration()
    {
        var cmd = RelayParser.Parse("tmute:steam_001:abc");
        Assert.That(cmd.IsValid, Is.False);
    }

    [Test]
    public void Parse_Tmute_EmptyTarget()
    {
        var cmd = RelayParser.Parse("tmute::60");
        Assert.That(cmd.IsValid, Is.False);
    }

    // -- ban ---------------------------------------------------------------

    [Test]
    public void Parse_Ban_ValidPayload()
    {
        var cmd = RelayParser.Parse("ban:steam_002");
        Assert.That(cmd.IsValid, Is.True);
        Assert.That(cmd.Type, Is.EqualTo(RelayCommandType.Ban));
        Assert.That(cmd.TargetSteamId, Is.EqualTo("steam_002"));
    }

    [Test]
    public void Parse_Ban_EmptyTarget()
    {
        var cmd = RelayParser.Parse("ban:");
        Assert.That(cmd.IsValid, Is.False);
        Assert.That(cmd.Error, Does.Contain("ban"));
    }

    // -- unmute ------------------------------------------------------------

    [Test]
    public void Parse_Unmute_ValidPayload()
    {
        var cmd = RelayParser.Parse("unmute:steam_003");
        Assert.That(cmd.IsValid, Is.True);
        Assert.That(cmd.Type, Is.EqualTo(RelayCommandType.Unmute));
        Assert.That(cmd.TargetSteamId, Is.EqualTo("steam_003"));
    }

    [Test]
    public void Parse_Unmute_ValidSteamId64()
    {
        var cmd = RelayParser.Parse("unmute:76561198000000002");
        Assert.That(cmd.IsValid, Is.True);
        Assert.That(cmd.TargetSteamId, Is.EqualTo("76561198000000002"));
    }

    [Test]
    public void Parse_Unmute_EmptyTarget()
    {
        var cmd = RelayParser.Parse("unmute:");
        Assert.That(cmd.IsValid, Is.False);
        Assert.That(cmd.Error, Does.Contain("unmute"));
    }

    // -- unknown / malformed -----------------------------------------------

    [Test]
    public void Parse_UnknownCommand()
    {
        var cmd = RelayParser.Parse("kick:steam_001");
        Assert.That(cmd.IsValid, Is.False);
        Assert.That(cmd.Error, Does.Contain("Unknown command"));
    }

    [Test]
    public void Parse_NoColon_Malformed()
    {
        var cmd = RelayParser.Parse("garbage");
        Assert.That(cmd.IsValid, Is.False);
        Assert.That(cmd.Error, Does.Contain("Malformed"));
    }

    [Test]
    public void Parse_EmptyString()
    {
        var cmd = RelayParser.Parse("");
        Assert.That(cmd.IsValid, Is.False);
    }

    [Test]
    public void Parse_NullString()
    {
        var cmd = RelayParser.Parse(null!);
        Assert.That(cmd.IsValid, Is.False);
    }
}
