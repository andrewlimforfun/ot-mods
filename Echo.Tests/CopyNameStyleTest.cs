using Echo.Core;
using static Echo.Core.NameStyleTransfer;

namespace Echo.Tests;

public class CopyNameStyleTest
{
    // --------------------- ParseSegments ---------------------

    [Test]
    public void ParseSegments_PlainText_SingleSegmentNoTags()
    {
        var segs = ParseSegments("hello");
        Assert.That(segs, Has.Count.EqualTo(1));
        Assert.That(segs[0].Tags, Is.EqualTo(""));
        Assert.That(segs[0].Text, Is.EqualTo("hello"));
    }

    [Test]
    public void ParseSegments_SingleTag_OneSegment()
    {
        var segs = ParseSegments("<color=red>abc");
        Assert.That(segs, Has.Count.EqualTo(1));
        Assert.That(segs[0].Tags, Is.EqualTo("<color=red>"));
        Assert.That(segs[0].Text, Is.EqualTo("abc"));
    }

    [Test]
    public void ParseSegments_MultipleTags_SplitsOnVisibleText()
    {
        // <t1><t2>a</t2><t3>b</t3><t4>c
        var segs = ParseSegments("<t1><t2>a</t2><t3>b</t3><t4>c");
        Assert.That(segs, Has.Count.EqualTo(3));

        Assert.That(segs[0].Tags, Is.EqualTo("<t1><t2>"));
        Assert.That(segs[0].Text, Is.EqualTo("a"));

        Assert.That(segs[1].Tags, Is.EqualTo("</t2><t3>"));
        Assert.That(segs[1].Text, Is.EqualTo("b"));

        Assert.That(segs[2].Tags, Is.EqualTo("</t3><t4>"));
        Assert.That(segs[2].Text, Is.EqualTo("c"));
    }

    [Test]
    public void ParseSegments_MultiCharSegments()
    {
        var segs = ParseSegments("<b>ab</b>cd");
        Assert.That(segs, Has.Count.EqualTo(2));
        Assert.That(segs[0].Tags, Is.EqualTo("<b>"));
        Assert.That(segs[0].Text, Is.EqualTo("ab"));
        Assert.That(segs[1].Tags, Is.EqualTo("</b>"));
        Assert.That(segs[1].Text, Is.EqualTo("cd"));
    }

    [Test]
    public void ParseSegments_TagOnlySegment()
    {
        // tag with no visible text at the end
        var segs = ParseSegments("a<b></b>");
        Assert.That(segs, Has.Count.EqualTo(2));
        Assert.That(segs[0].Tags, Is.EqualTo(""));
        Assert.That(segs[0].Text, Is.EqualTo("a"));
        Assert.That(segs[1].Tags, Is.EqualTo("<b></b>"));
        Assert.That(segs[1].Text, Is.EqualTo(""));
    }

    [Test]
    public void ParseSegments_Empty_ReturnsEmpty()
    {
        var segs = ParseSegments("");
        Assert.That(segs, Is.Empty);
    }

    [Test]
    public void ParseSegments_UnclosedAngleBracket_TreatedAsText()
    {
        var segs = ParseSegments("a<broken");
        Assert.That(segs, Has.Count.EqualTo(2));
        Assert.That(segs[0].Tags, Is.EqualTo(""));
        Assert.That(segs[0].Text, Is.EqualTo("a"));
        Assert.That(segs[1].Tags, Is.EqualTo(""));
        Assert.That(segs[1].Text, Is.EqualTo("<broken"));
    }

    // --------------------- ApplyPositional ---------------------

    [Test]
    public void Positional_ExactLengthMatch()
    {
        // target: <t1>a<t2>b<t3>c  -> 3 segments, 3 chars
        var segs = ParseSegments("<t1>a<t2>b<t3>c");
        string result = ApplyPositional(segs, "xyz");
        Assert.That(result, Is.EqualTo("<t1>x<t2>y<t3>z"));
    }

    [Test]
    public void Positional_MyNameLonger_OverflowToLastSegment()
    {
        // target: <t1>a<t2>b  -> 2 segments with 1 char each
        var segs = ParseSegments("<t1>a<t2>b");
        string result = ApplyPositional(segs, "xyzw");
        // x maps to seg0, y+z+w overflow into last seg1
        Assert.That(result, Is.EqualTo("<t1>x<t2>yzw"));
    }

    [Test]
    public void Positional_MyNameShorter_LaterSegmentsEmpty()
    {
        // target: <t1>ab<t2>cd<t3>ef  -> 3 segments, 6 chars total
        var segs = ParseSegments("<t1>ab<t2>cd<t3>ef");
        string result = ApplyPositional(segs, "xy");
        // x,y fill seg0 (2 chars), seg1 and seg2 get nothing but keep tags
        Assert.That(result, Is.EqualTo("<t1>xy<t2><t3>"));
    }

    [Test]
    public void Positional_PlainTargetName_NoTags()
    {
        var segs = ParseSegments("abc");
        string result = ApplyPositional(segs, "xyz");
        Assert.That(result, Is.EqualTo("xyz"));
    }

    [Test]
    public void Positional_EmptySegments_ReturnsMyChars()
    {
        string result = ApplyPositional(new System.Collections.Generic.List<StyledSegment>(), "xyz");
        Assert.That(result, Is.EqualTo("xyz"));
    }

    [Test]
    public void Positional_SingleChar_SingleSegment()
    {
        var segs = ParseSegments("<color=#FF0000>X");
        string result = ApplyPositional(segs, "A");
        Assert.That(result, Is.EqualTo("<color=#FF0000>A"));
    }

    [Test]
    public void Positional_UserExample()
    {
        // target: <tag1><tag2>a</tag2><tag3>b</tag3><tag4>c
        // my name (cleaned): xyz
        // expected: <tag1><tag2>x</tag2><tag3>y</tag3><tag4>z
        var segs = ParseSegments("<tag1><tag2>a</tag2><tag3>b</tag3><tag4>c");
        string result = ApplyPositional(segs, "xyz");
        Assert.That(result, Is.EqualTo("<tag1><tag2>x</tag2><tag3>y</tag3><tag4>z"));
    }

    [Test]
    public void Positional_MultiCharSegments_MapsByPosition()
    {
        // target: <b>abc<i>de -> seg0 has 3 chars, seg1 has 2
        var segs = ParseSegments("<b>abc<i>de");
        string result = ApplyPositional(segs, "12345");
        Assert.That(result, Is.EqualTo("<b>123<i>45"));
    }

    // --------------------- ApplyEven ---------------------

    [Test]
    public void Even_ExactlyDivisible()
    {
        // 3 segments, 6 chars -> 2 each
        var segs = ParseSegments("<a>x<b>y<c>z");
        string result = ApplyEven(segs, "123456");
        Assert.That(result, Is.EqualTo("<a>12<b>34<c>56"));
    }

    [Test]
    public void Even_RemainderDistributed()
    {
        // 3 segments, 7 chars -> 3,2,2
        var segs = ParseSegments("<a>x<b>y<c>z");
        string result = ApplyEven(segs, "1234567");
        Assert.That(result, Is.EqualTo("<a>123<b>45<c>67"));
    }

    [Test]
    public void Even_FewerCharsThanSegments()
    {
        // 3 segments, 2 chars -> 1,1,0
        var segs = ParseSegments("<a>x<b>y<c>z");
        string result = ApplyEven(segs, "AB");
        Assert.That(result, Is.EqualTo("<a>A<b>B<c>"));
    }

    [Test]
    public void Even_SingleSegment()
    {
        var segs = ParseSegments("<b>abc");
        string result = ApplyEven(segs, "XY");
        Assert.That(result, Is.EqualTo("<b>XY"));
    }

    [Test]
    public void Even_TagOnlySegments_Skipped()
    {
        // seg0: tags-only, seg1: has text, seg2: has text
        var segs = ParseSegments("<a></a><b>x<c>y");
        // Only 2 text segments: 3 chars -> 2,1
        string result = ApplyEven(segs, "ABC");
        Assert.That(result, Is.EqualTo("<a></a><b>AB<c>C"));
    }

    [Test]
    public void Even_AllTagsOnly_DumpsToEnd()
    {
        var segs = new System.Collections.Generic.List<StyledSegment>
        {
            new StyledSegment("<a>", ""),
            new StyledSegment("<b>", ""),
        };
        string result = ApplyEven(segs, "xyz");
        Assert.That(result, Is.EqualTo("<a><b>xyz"));
    }

    [Test]
    public void Even_EmptySegments_ReturnsMyChars()
    {
        string result = ApplyEven(new System.Collections.Generic.List<StyledSegment>(), "xyz");
        Assert.That(result, Is.EqualTo("xyz"));
    }

    [Test]
    public void Even_OneCharPerSegment()
    {
        // 3 segments, 3 chars -> exactly 1 each
        var segs = ParseSegments("<a>x<b>y<c>z");
        string result = ApplyEven(segs, "ABC");
        Assert.That(result, Is.EqualTo("<a>A<b>B<c>C"));
    }

    // --------------------- Edge cases ---------------------

    [Test]
    public void Positional_NestedTags()
    {
        var segs = ParseSegments("<color=red><b>ab</b></color>");
        string result = ApplyPositional(segs, "XY");
        Assert.That(result, Is.EqualTo("<color=red><b>XY</b></color>"));
    }

    [Test]
    public void Positional_SingleCharMyName()
    {
        var segs = ParseSegments("<a>xx<b>yy");
        string result = ApplyPositional(segs, "Z");
        // Z fills first 1 of seg0 (2 chars), but only 1 char available; seg1 empty
        Assert.That(result, Is.EqualTo("<a>Z<b>"));
    }

    [Test]
    public void Even_SingleCharMyName_GoesToFirstSegment()
    {
        var segs = ParseSegments("<a>xx<b>yy");
        string result = ApplyEven(segs, "Z");
        // 2 text segments, 1 char: base=0, remainder=1 -> first gets 1, second gets 0
        Assert.That(result, Is.EqualTo("<a>Z<b>"));
    }

    [Test]
    public void Positional_ClosingTagsPreserved()
    {
        var segs = ParseSegments("<color=#FF0000>a</color><color=#00FF00>b</color>");
        string result = ApplyPositional(segs, "XY");
        Assert.That(result, Is.EqualTo("<color=#FF0000>X</color><color=#00FF00>Y</color>"));
    }
}
