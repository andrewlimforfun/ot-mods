using System;
using System.Collections.Generic;
using System.Text;

namespace Echo.Core
{
    /// <summary>
    /// Transfers TMP tag styling from one name onto another name's characters.
    /// </summary>
    internal static class NameStyleTransfer
    {
        /// <summary>
        /// Parses a TMP-tagged string into segments.
        /// Each segment is (leading tags, visible characters).
        /// Anything inside angle brackets is treated as a tag.
        /// </summary>
        internal static List<StyledSegment> ParseSegments(string input)
        {
            var segments = new List<StyledSegment>();
            var tagBuffer = new StringBuilder();
            var textBuffer = new StringBuilder();

            int i = 0;
            while (i < input.Length)
            {
                if (input[i] == '<')
                {
                    if (textBuffer.Length > 0)
                    {
                        segments.Add(new StyledSegment(tagBuffer.ToString(), textBuffer.ToString()));
                        tagBuffer.Clear();
                        textBuffer.Clear();
                    }

                    int close = input.IndexOf('>', i);
                    if (close >= 0)
                    {
                        tagBuffer.Append(input, i, close - i + 1);
                        i = close + 1;
                    }
                    else
                    {
                        textBuffer.Append(input, i, input.Length - i);
                        i = input.Length;
                    }
                }
                else
                {
                    textBuffer.Append(input[i]);
                    i++;
                }
            }

            if (tagBuffer.Length > 0 || textBuffer.Length > 0)
                segments.Add(new StyledSegment(tagBuffer.ToString(), textBuffer.ToString()));

            return segments;
        }

        /// <summary>
        /// Positional mode: map characters 1:1 by position.
        /// Overflow goes into the last segment.
        /// </summary>
        internal static string ApplyPositional(List<StyledSegment> segments, string myChars)
        {
            if (segments.Count == 0)
                return myChars;

            var sb = new StringBuilder();
            int charIdx = 0;

            for (int s = 0; s < segments.Count; s++)
            {
                sb.Append(segments[s].Tags);
                int take = segments[s].Text.Length;

                if (s == segments.Count - 1)
                {
                    if (charIdx < myChars.Length)
                        sb.Append(myChars, charIdx, myChars.Length - charIdx);
                }
                else
                {
                    int end = Math.Min(charIdx + take, myChars.Length);
                    if (charIdx < end)
                        sb.Append(myChars, charIdx, end - charIdx);
                    charIdx = end;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Even distribution mode: spread characters as evenly as possible
        /// across segments that originally had visible text.
        /// </summary>
        internal static string ApplyEven(List<StyledSegment> segments, string myChars)
        {
            if (segments.Count == 0)
                return myChars;

            int textSegments = 0;
            foreach (StyledSegment seg in segments)
            {
                if (seg.Text.Length > 0)
                    textSegments++;
            }

            if (textSegments == 0)
            {
                var fallback = new StringBuilder();
                foreach (StyledSegment seg in segments)
                    fallback.Append(seg.Tags);
                fallback.Append(myChars);
                return fallback.ToString();
            }

            int baseCount = myChars.Length / textSegments;
            int remainder = myChars.Length % textSegments;

            var sb = new StringBuilder();
            int charIdx = 0;
            int textSegIdx = 0;

            foreach (StyledSegment seg in segments)
            {
                sb.Append(seg.Tags);
                if (seg.Text.Length > 0)
                {
                    int take = baseCount + (textSegIdx < remainder ? 1 : 0);
                    int end = Math.Min(charIdx + take, myChars.Length);
                    if (charIdx < end)
                        sb.Append(myChars, charIdx, end - charIdx);
                    charIdx = end;
                    textSegIdx++;
                }
            }

            if (charIdx < myChars.Length)
                sb.Append(myChars, charIdx, myChars.Length - charIdx);

            return sb.ToString();
        }

        /// <summary>
        /// Even distribution mode that treats non-ASCII characters in the target as part of the style.
        /// Non-ASCII chars are kept in place; only ASCII character slots are filled with the user's chars.
        /// </summary>
        internal static string ApplyEvenKeepNonAscii(List<StyledSegment> segments, string myChars)
        {
            if (segments.Count == 0)
                return myChars;

            int asciiSlots = 0;
            foreach (StyledSegment seg in segments)
            {
                foreach (char c in seg.Text)
                {
                    if (c <= '\x7F')
                        asciiSlots++;
                }
            }

            if (asciiSlots == 0)
            {
                // No ASCII slots to fill - just reconstruct with tags + non-ASCII text
                var fallback = new StringBuilder();
                foreach (StyledSegment seg in segments)
                {
                    fallback.Append(seg.Tags);
                    fallback.Append(seg.Text);
                }
                fallback.Append(myChars);
                return fallback.ToString();
            }

            // Distribute user chars evenly across ASCII slots in each segment
            int totalSegmentsWithAscii = 0;
            foreach (StyledSegment seg in segments)
            {
                foreach (char c in seg.Text)
                {
                    if (c <= '\x7F')
                    {
                        totalSegmentsWithAscii++;
                        break;
                    }
                }
            }

            int baseCount = myChars.Length / totalSegmentsWithAscii;
            int remainder = myChars.Length % totalSegmentsWithAscii;

            var sb = new StringBuilder();
            int charIdx = 0;
            int segIdx = 0;

            foreach (StyledSegment seg in segments)
            {
                sb.Append(seg.Tags);

                bool hasAscii = false;
                foreach (char c in seg.Text)
                {
                    if (c <= '\x7F')
                    {
                        hasAscii = true;
                        break;
                    }
                }

                if (!hasAscii)
                {
                    // Keep all non-ASCII text as-is
                    sb.Append(seg.Text);
                    continue;
                }

                int take = baseCount + (segIdx < remainder ? 1 : 0);
                int charsWritten = 0;

                foreach (char c in seg.Text)
                {
                    if (c > '\x7F')
                    {
                        // Non-ASCII: keep as part of style
                        sb.Append(c);
                    }
                    else
                    {
                        // ASCII slot: fill with user's char
                        if (charsWritten < take && charIdx < myChars.Length)
                        {
                            sb.Append(myChars[charIdx]);
                            charIdx++;
                            charsWritten++;
                        }
                    }
                }

                // If we still have chars to place for this segment (fewer ASCII slots than take)
                while (charsWritten < take && charIdx < myChars.Length)
                {
                    sb.Append(myChars[charIdx]);
                    charIdx++;
                    charsWritten++;
                }

                segIdx++;
            }

            // Append any remaining chars
            if (charIdx < myChars.Length)
                sb.Append(myChars, charIdx, myChars.Length - charIdx);

            return sb.ToString();
        }

        internal readonly struct StyledSegment
        {
            public readonly string Tags;
            public readonly string Text;

            public StyledSegment(string tags, string text)
            {
                Tags = tags;
                Text = text;
            }
        }
    }
}
