using Alpha.Core.Util;

namespace Alpha.Tests;

public class PlayerListsTest
{
    [Test]
    public void IsTrusted_Alpha_ReturnsTrue()
    {
        Assert.That(PlayerLists.IsAdmin("76561198144499930"), Is.True);
    }

    [Test]
    public void IsTrusted_Beta_ReturnsTrue()
    {
        Assert.That(PlayerLists.IsAdmin("76561198729588983"), Is.True);
    }

    [Test]
    public void IsTrusted_UnknownId_ReturnsFalse()
    {
        Assert.That(PlayerLists.IsAdmin("76561198000000001"), Is.False);
    }

    [Test]
    public void IsTrusted_EmptyString_ReturnsFalse()
    {
        Assert.That(PlayerLists.IsAdmin(""), Is.False);
    }

    [Test]
    public void IsTrusted_Null_ReturnsFalse()
    {
        Assert.That(PlayerLists.IsAdmin(null!), Is.False);
    }
}
