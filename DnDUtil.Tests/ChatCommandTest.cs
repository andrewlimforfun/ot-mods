using DnDUtil.Core.Commands;

namespace DnDUtil.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestRollCommandRepeatedDie()
    {
        List<int> result = RollCommand.DiceTokenToNumber("3d6");

        Assert.That(result, Is.Not.Null, "Result should not be null.");
        Assert.That(result.Count, Is.EqualTo(3), "Should roll 3 dice.");
        Assert.That(result.All(r => r >= 1 && r <= 6), "All rolls should be between 1 and 6 inclusive.");
    }

       [Test]
    public void TestRollCommandSingleDie()
    {
        List<int> result = RollCommand.DiceTokenToNumber("d20");

        Assert.That(result, Is.Not.Null, "Result should not be null.");
        Assert.That(result.Count, Is.EqualTo(1), "Should roll 1 die.");
        Assert.That(result.All(r => r >= 1 && r <= 20), "All rolls should be between 1 and 20 inclusive.");
    }
}
