using System.Collections.Generic;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Chunks text by breaking at the last whitespace before the limit.
    /// Falls back to a hard cut only when a single word exceeds <c>maxLength</c>.
    /// Leading/trailing whitespace on each chunk is trimmed.
    /// </summary>
    public class WordBoundaryChunker : IStringChunker
    {
        public IEnumerable<string> Chunk(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            int start = 0;
            while (start < text.Length)
            {
                int remaining = text.Length - start;
                if (remaining <= maxLength)
                {
                    yield return text[start..].Trim();
                    yield break;
                }

                // Find the last space within the window
                int cut = text.LastIndexOf(' ', start + maxLength - 1, maxLength);
                if (cut <= start)
                {
                    // single word longer than limit - hard cut, no char to skip
                    yield return text[start..(start + maxLength)];
                    start += maxLength;
                }
                else
                {
                    yield return text[start..cut].Trim();
                    start = cut + 1; // skip the space
                }
            }
        }
    }
}
