using Hush.Core;

namespace Hush.Tests;

public class ChatFilterManagerTest
{
    private ChatFilterManager _filter = null!;

    [SetUp]
    public void Setup()
    {
        _filter = new ChatFilterManager();
    }

    // -- Add / Remove words ------------------------------------------------

    [Test]
    public void Add_NewWord_ReturnsTrue()
    {
        Assert.That(_filter.Add("badword"), Is.True);
    }

    [Test]
    public void Add_DuplicateWord_ReturnsFalse()
    {
        _filter.Add("badword");
        Assert.That(_filter.Add("badword"), Is.False);
    }

    [Test]
    public void Add_NullOrWhitespace_ReturnsFalse()
    {
        Assert.That(_filter.Add(null!), Is.False);
        Assert.That(_filter.Add(""), Is.False);
        Assert.That(_filter.Add("   "), Is.False);
    }

    [Test]
    public void Add_TrimsWhitespace()
    {
        _filter.Add("  bad  ");
        string[] words = _filter.GetWords();
        Assert.That(words, Has.Length.EqualTo(1));
        Assert.That(words[0], Is.EqualTo("bad"));
    }

    [Test]
    public void Remove_ExistingWord_ReturnsTrue()
    {
        _filter.Add("badword");
        Assert.That(_filter.Remove("badword"), Is.True);
    }

    [Test]
    public void Remove_MissingWord_ReturnsFalse()
    {
        Assert.That(_filter.Remove("nope"), Is.False);
    }

    [Test]
    public void Count_ReflectsWordsAndPatterns()
    {
        _filter.Add("word1");
        _filter.Add("word2");
        _filter.AddPattern(@"f+u+c+k");
        Assert.That(_filter.Count, Is.EqualTo(3));
    }

    // -- Add / Remove patterns ---------------------------------------------

    [Test]
    public void AddPattern_ValidRegex_ReturnsTrue()
    {
        Assert.That(_filter.AddPattern(@"f+u+c+k"), Is.True);
    }

    [Test]
    public void AddPattern_InvalidRegex_ReturnsFalse()
    {
        Assert.That(_filter.AddPattern("[invalid"), Is.False);
    }

    [Test]
    public void AddPattern_Duplicate_ReturnsFalse()
    {
        _filter.AddPattern(@"test\d+");
        Assert.That(_filter.AddPattern(@"test\d+"), Is.False);
    }

    [Test]
    public void RemovePattern_Existing_ReturnsTrue()
    {
        _filter.AddPattern(@"abc");
        Assert.That(_filter.RemovePattern("abc"), Is.True);
    }

    [Test]
    public void RemovePattern_Missing_ReturnsFalse()
    {
        Assert.That(_filter.RemovePattern("nope"), Is.False);
    }

    // -- Apply: censor mode ------------------------------------------------

    [Test]
    public void Apply_CensorMode_ReplacesBadWord()
    {
        _filter.Action = FilterAction.Censor;
        _filter.Add("bad");
        var result = _filter.Apply("this is bad stuff");
        Assert.That(result.WasModified, Is.True);
        Assert.That(result.WasBlocked, Is.False);
        Assert.That(result.Text, Is.EqualTo("this is *** stuff"));
    }

    [Test]
    public void Apply_CensorMode_CaseInsensitive()
    {
        _filter.Action = FilterAction.Censor;
        _filter.Add("bad");
        var result = _filter.Apply("this is BAD stuff");
        Assert.That(result.WasModified, Is.True);
        Assert.That(result.Text, Is.EqualTo("this is *** stuff"));
    }

    [Test]
    public void Apply_CensorMode_RespectsWordBoundary()
    {
        _filter.Action = FilterAction.Censor;
        _filter.Add("bad");
        var result = _filter.Apply("badge is not bad");
        Assert.That(result.WasModified, Is.True);
        // "badge" should NOT be censored (word boundary), only standalone "bad"
        Assert.That(result.Text, Does.StartWith("badge"));
        Assert.That(result.Text, Does.EndWith("***"));
    }

    [Test]
    public void Apply_CensorMode_CustomCensorChar()
    {
        _filter.Action = FilterAction.Censor;
        _filter.CensorChar = '#';
        _filter.Add("bad");
        var result = _filter.Apply("this is bad");
        Assert.That(result.Text, Is.EqualTo("this is ###"));
    }

    [Test]
    public void Apply_CensorMode_MultipleWords()
    {
        _filter.Action = FilterAction.Censor;
        _filter.Add("foo");
        _filter.Add("bar");
        var result = _filter.Apply("foo and bar together");
        Assert.That(result.WasModified, Is.True);
        Assert.That(result.Text, Is.EqualTo("*** and *** together"));
    }

    [Test]
    public void Apply_CensorMode_Pattern_MatchesInsideWords()
    {
        _filter.Action = FilterAction.Censor;
        _filter.AddPattern(@"f+u+c+k");
        var result = _filter.Apply("what the fuuuck");
        Assert.That(result.WasModified, Is.True);
        Assert.That(result.Text, Does.Not.Contain("fuuuck"));
    }

    // -- Apply: block mode -------------------------------------------------

    [Test]
    public void Apply_BlockMode_BlocksBadWord()
    {
        _filter.Action = FilterAction.Block;
        _filter.Add("bad");
        var result = _filter.Apply("this is bad");
        Assert.That(result.WasBlocked, Is.True);
    }

    [Test]
    public void Apply_BlockMode_PassesCleanMessage()
    {
        _filter.Action = FilterAction.Block;
        _filter.Add("bad");
        var result = _filter.Apply("this is fine");
        Assert.That(result.WasBlocked, Is.False);
        Assert.That(result.Text, Is.EqualTo("this is fine"));
    }

    // -- Apply: disabled / empty -------------------------------------------

    [Test]
    public void Apply_Disabled_ReturnsUnchanged()
    {
        _filter.Action = FilterAction.Censor;
        _filter.Add("bad");
        _filter.Enabled = false;
        var result = _filter.Apply("this is bad");
        Assert.That(result.WasModified, Is.False);
        Assert.That(result.WasBlocked, Is.False);
        Assert.That(result.Text, Is.EqualTo("this is bad"));
    }

    [Test]
    public void Apply_EmptyFilter_ReturnsUnchanged()
    {
        var result = _filter.Apply("anything goes");
        Assert.That(result.WasModified, Is.False);
        Assert.That(result.Text, Is.EqualTo("anything goes"));
    }

    [Test]
    public void Apply_NullOrEmpty_ReturnsUnchanged()
    {
        _filter.Add("bad");
        Assert.That(_filter.Apply(null!).WasModified, Is.False);
        Assert.That(_filter.Apply("").WasModified, Is.False);
    }

    // -- Clear -------------------------------------------------------------

    [Test]
    public void Clear_RemovesAllEntries()
    {
        _filter.Add("a");
        _filter.Add("b");
        _filter.AddPattern(@"\d+");
        _filter.Clear();
        Assert.That(_filter.Count, Is.EqualTo(0));
        Assert.That(_filter.GetWords(), Is.Empty);
        Assert.That(_filter.GetPatterns(), Is.Empty);
    }

    [Test]
    public void Clear_FilterNoLongerMatches()
    {
        _filter.Action = FilterAction.Block;
        _filter.Add("bad");
        _filter.Clear();
        var result = _filter.Apply("bad");
        Assert.That(result.WasBlocked, Is.False);
    }

    // -- Save / Load -------------------------------------------------------

    [Test]
    public void SaveAndLoad_RoundTrips()
    {
        _filter.Add("word1");
        _filter.Add("word2");
        _filter.AddPattern(@"test\d+");

        string path = Path.Combine(Path.GetTempPath(), $"hush_filter_test_{Guid.NewGuid()}.json");
        try
        {
            _filter.Save(path);

            var loaded = new ChatFilterManager();
            loaded.Load(path);

            Assert.That(loaded.GetWords(), Is.EquivalentTo(new[] { "word1", "word2" }));
            Assert.That(loaded.GetPatterns(), Is.EquivalentTo(new[] { @"test\d+" }));
            Assert.That(loaded.Count, Is.EqualTo(3));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Load_MissingFile_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _filter.Load(@"C:\nonexistent\path\file.json"));
    }

    [Test]
    public void Load_InvalidRegexInFile_SkipsInvalidKeepsValid()
    {
        // Write a config with one valid and one invalid pattern
        string json = """
        {
            "Words": ["good"],
            "Patterns": ["valid\\d+", "[broken"]
        }
        """;
        string path = Path.Combine(Path.GetTempPath(), $"hush_filter_test_{Guid.NewGuid()}.json");
        try
        {
            File.WriteAllText(path, json);
            _filter.Load(path);
            Assert.That(_filter.GetWords(), Is.EquivalentTo(new[] { "good" }));
            Assert.That(_filter.GetPatterns(), Is.EquivalentTo(new[] { @"valid\d+" }));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
