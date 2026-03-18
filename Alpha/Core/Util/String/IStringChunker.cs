using System.Collections.Generic;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Breaks a string into segments no longer than a specified maximum length.
    /// </summary>
    public interface IStringChunker
    {
        /// <summary>
        /// Splits <paramref name="text"/> into a sequence of chunks, each at most
        /// <paramref name="maxLength"/> characters long. The original text is fully
        /// preserved across all chunks (no characters are dropped).
        /// </summary>
        /// <param name="text">The text to chunk. Returns empty enumerable when null or empty.</param>
        /// <param name="maxLength">Maximum number of characters per chunk. Must be &gt; 0.</param>
        /// <returns>Ordered sequence of chunks that reconstruct the original text when joined.</returns>
        IEnumerable<string> Chunk(string text, int maxLength);
    }
}
