using Alpha.Core.Util;

namespace Alpha.Tests;

public class StringObfuscatorTest
{
    // -- roundtrip ---------------------------------------------------------

    [Test]
    public void Roundtrip_AsciiString()
    {
        string original = "hello world";
        Assert.That(StringObfuscator.Deobfuscate(StringObfuscator.Obfuscate(original)), Is.EqualTo(original));
    }

    [Test]
    public void Roundtrip_SteamId()
    {
        string original = "76561198123456789";
        Assert.That(StringObfuscator.Deobfuscate(StringObfuscator.Obfuscate(original)), Is.EqualTo(original));
    }

    [Test]
    public void Roundtrip_EmptyString()
    {
        Assert.That(StringObfuscator.Deobfuscate(StringObfuscator.Obfuscate("")), Is.EqualTo(""));
    }

    [Test]
    public void Roundtrip_UnicodeString()
    {
        string original = "こんにちは 🎮";
        Assert.That(StringObfuscator.Deobfuscate(StringObfuscator.Obfuscate(original)), Is.EqualTo(original));
    }

    [Test]
    public void Roundtrip_StringLongerThanKey()
    {
        // Key is 8 bytes; this string is longer to exercise the wrap-around
        string original = "abcdefghijklmnopqrstuvwxyz";
        Assert.That(StringObfuscator.Deobfuscate(StringObfuscator.Obfuscate(original)), Is.EqualTo(original));
    }

    [Test]
    public void Roundtrip_StringExactlyKeyLength()
    {
        string original = "12345678"; // 8 ASCII chars = 8 bytes
        Assert.That(StringObfuscator.Deobfuscate(StringObfuscator.Obfuscate(original)), Is.EqualTo(original));
    }

    [Test]
    public void Roundtrip_Multiline()
    {
        string original = "line1\nline2\r\nline3";
        Assert.That(StringObfuscator.Deobfuscate(StringObfuscator.Obfuscate(original)), Is.EqualTo(original));
    }

    // -- obfuscated form ---------------------------------------------------

    [Test]
    public void Obfuscate_ReturnsValidBase64()
    {
        string encoded = StringObfuscator.Obfuscate("test");
        Assert.DoesNotThrow(() => Convert.FromBase64String(encoded));
    }

    [Test]
    public void Obfuscate_NotEqualToOriginal()
    {
        string original = "76561198123456789";
        Assert.That(StringObfuscator.Obfuscate(original), Is.Not.EqualTo(original));
    }

    [Test]
    public void Obfuscate_IsDeterministic()
    {
        string original = "76561198123456789";
        string first = StringObfuscator.Obfuscate(original);
        string second = StringObfuscator.Obfuscate(original);
        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Obfuscate_DifferentInputs_ProduceDifferentOutput()
    {
        Assert.That(StringObfuscator.Obfuscate("76561198000000001"),
            Is.Not.EqualTo(StringObfuscator.Obfuscate("76561198000000002")));
    }

    [Test]
    public void Obfuscate_DoesNotContainOriginal()
    {
        // Encoded form should not contain the plain Steam ID as a substring
        string original = "76561198123456789";
        Assert.That(StringObfuscator.Obfuscate(original), Does.Not.Contain(original));
    }

    // -- null guards -------------------------------------------------------

    [Test]
    public void Obfuscate_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => StringObfuscator.Obfuscate(null!));
    }

    [Test]
    public void Deobfuscate_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => StringObfuscator.Deobfuscate(null!));
    }

    [Test]
    public void Deobfuscate_InvalidBase64_Throws()
    {
        Assert.Throws<FormatException>(() => StringObfuscator.Deobfuscate("not-valid-base64!!!"));
    }
}
