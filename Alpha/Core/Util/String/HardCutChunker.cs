using System.Collections.Generic;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Chunks text by slicing at exactly <c>maxLength</c> characters with no regard
    /// for word boundaries. Fastest strategy; may cut words mid-character.
    /// </summary>
    public class HardCutChunker : IStringChunker
    {
        public IEnumerable<string> Chunk(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            for (int start = 0; start < text.Length; start += maxLength)
                yield return text.Substring(start, System.Math.Min(maxLength, text.Length - start));
        }
    }
}
