using Hush.Core;

namespace Hush.Tests;

public class DurationFormatterTest
{
    [TestCase(0, "0s")]
    [TestCase(1, "1s")]
    [TestCase(30, "30s")]
    [TestCase(59, "59s")]
    public void Seconds(int seconds, string expected)
    {
        Assert.That(DurationFormatter.Format(TimeSpan.FromSeconds(seconds)), Is.EqualTo(expected));
    }

    [TestCase(60, "1m")]
    [TestCase(90, "1m")]     // 1.5 min → truncated to 1m
    [TestCase(300, "5m")]
    [TestCase(3540, "59m")]  // 59 minutes
    public void Minutes(int seconds, string expected)
    {
        Assert.That(DurationFormatter.Format(TimeSpan.FromSeconds(seconds)), Is.EqualTo(expected));
    }

    [Test]
    public void Hours_Exact()
    {
        Assert.That(DurationFormatter.Format(TimeSpan.FromHours(2)), Is.EqualTo("2h"));
    }

    [Test]
    public void Hours_WithMinutes()
    {
        Assert.That(DurationFormatter.Format(new TimeSpan(2, 30, 0)), Is.EqualTo("2h 30m"));
    }

    [Test]
    public void Hours_ZeroMinutes()
    {
        Assert.That(DurationFormatter.Format(new TimeSpan(5, 0, 0)), Is.EqualTo("5h"));
    }

    [Test]
    public void Days()
    {
        Assert.That(DurationFormatter.Format(TimeSpan.FromDays(3)), Is.EqualTo("3d"));
    }

    [Test]
    public void Days_LargeValue()
    {
        Assert.That(DurationFormatter.Format(TimeSpan.FromDays(365)), Is.EqualTo("365d"));
    }
}
