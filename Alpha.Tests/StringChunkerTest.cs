using Alpha.Core.Util;

namespace Alpha.Tests;

public class StringChunkerTest
{
    // A set of realistic long English sentences used across tests
    private const string ShortSentence = "The quick brown fox jumps over the lazy dog.";
    private const string MediumParagraph =
        "In the beginning, the universe was created. " +
        "This has made a lot of people very angry and has been widely regarded as a bad move. " +
        "Many races believe that it was a mistake to be so forthright about the origin of all things, " +
        "because it tends to cause unnecessary existential dread in the population.";
    private const string LongWord = "Pneumonoultramicroscopicsilicovolcanoconiosis"; // 45 chars
    private const string ExactFit = "Hello"; // exactly 5 chars

    // --------------------------------------------------------------------------
    // WordBoundaryChunker
    // --------------------------------------------------------------------------

    [Test]
    public void WordBoundary_NullOrEmpty_ReturnsEmpty()
    {
        var chunker = new WordBoundaryChunker();
        Assert.That(chunker.Chunk(null!, 50), Is.Empty);
        Assert.That(chunker.Chunk(string.Empty, 50), Is.Empty);
    }

    [Test]
    public void WordBoundary_TextShorterThanLimit_ReturnsSingleChunk()
    {
        var chunker = new WordBoundaryChunker();
        var result = chunker.Chunk(ShortSentence, 200).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(ShortSentence));
    }

    [Test]
    public void WordBoundary_TextExactlyLimit_ReturnsSingleChunk()
    {
        var chunker = new WordBoundaryChunker();
        var result = chunker.Chunk(ExactFit, 5).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(ExactFit));
    }

    [Test]
    public void WordBoundary_ChunksDoNotExceedMaxLength()
    {
        var chunker = new WordBoundaryChunker();
        foreach (var chunk in chunker.Chunk(MediumParagraph, 50))
            Assert.That(chunk.Length, Is.LessThanOrEqualTo(50),
                $"Chunk exceeded limit: '{chunk}'");
    }

    [Test]
    public void WordBoundary_AllTextPreserved()
    {
        var chunker = new WordBoundaryChunker();
        var chunks = chunker.Chunk(MediumParagraph, 50).ToList();
        // Rejoin with a space — each split removed exactly one space
        string rejoined = string.Join(" ", chunks);
        Assert.That(rejoined, Is.EqualTo(MediumParagraph));
    }

    [Test]
    public void WordBoundary_ChunksDontStartOrEndWithSpace()
    {
        var chunker = new WordBoundaryChunker();
        foreach (var chunk in chunker.Chunk(MediumParagraph, 60))
        {
            Assert.That(chunk, Does.Not.StartWith(" "), $"Chunk starts with space: '{chunk}'");
            Assert.That(chunk, Does.Not.EndWith(" "), $"Chunk ends with space: '{chunk}'");
        }
    }

    [Test]
    public void WordBoundary_SingleLongWordFallsBackToHardCut()
    {
        // Word is 45 chars; limit is 20 — must still produce chunks ≤ 20
        var chunker = new WordBoundaryChunker();
        var chunks = chunker.Chunk(LongWord, 20).ToList();
        Assert.That(chunks, Has.Count.GreaterThan(1));
        foreach (var chunk in chunks)
            Assert.That(chunk.Length, Is.LessThanOrEqualTo(20));
        Assert.That(string.Concat(chunks), Is.EqualTo(LongWord));
    }

    [Test]
    public void WordBoundary_MultipleChunksCount()
    {
        var chunker = new WordBoundaryChunker();
        // MediumParagraph is ~270 chars; limit 50 → expect at least 5 chunks
        var chunks = chunker.Chunk(MediumParagraph, 50).ToList();
        Assert.That(chunks.Count, Is.GreaterThanOrEqualTo(5));
    }

    // --------------------------------------------------------------------------
    // HardCutChunker
    // --------------------------------------------------------------------------

    [Test]
    public void HardCut_NullOrEmpty_ReturnsEmpty()
    {
        var chunker = new HardCutChunker();
        Assert.That(chunker.Chunk(null!, 50), Is.Empty);
        Assert.That(chunker.Chunk(string.Empty, 50), Is.Empty);
    }

    [Test]
    public void HardCut_TextShorterThanLimit_ReturnsSingleChunk()
    {
        var chunker = new HardCutChunker();
        var result = chunker.Chunk(ShortSentence, 200).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(ShortSentence));
    }

    [Test]
    public void HardCut_ExactMultiple_EvenChunks()
    {
        var chunker = new HardCutChunker();
        var result = chunker.Chunk("abcdefghij", 5).ToList();
        Assert.That(result, Is.EqualTo(new[] { "abcde", "fghij" }));
    }

    [Test]
    public void HardCut_AllTextPreserved()
    {
        var chunker = new HardCutChunker();
        var chunks = chunker.Chunk(MediumParagraph, 50).ToList();
        Assert.That(string.Concat(chunks), Is.EqualTo(MediumParagraph));
    }

    [Test]
    public void HardCut_AllChunksExactlyMaxLength_ExceptLast()
    {
        var chunker = new HardCutChunker();
        var chunks = chunker.Chunk(MediumParagraph, 50).ToList();
        for (int i = 0; i < chunks.Count - 1; i++)
            Assert.That(chunks[i].Length, Is.EqualTo(50), $"Chunk {i} was not exactly 50 chars");
        Assert.That(chunks[^1].Length, Is.LessThanOrEqualTo(50));
    }

    [Test]
    public void HardCut_ChunkCount_MatchesCeiling()
    {
        var chunker = new HardCutChunker();
        int maxLen = 30;
        var chunks = chunker.Chunk(MediumParagraph, maxLen).ToList();
        int expected = (int)Math.Ceiling((double)MediumParagraph.Length / maxLen);
        Assert.That(chunks.Count, Is.EqualTo(expected));
    }
}
