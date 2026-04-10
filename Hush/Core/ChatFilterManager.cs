using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace Hush.Core
{
    /// <summary>
    /// Determines what the filter should do when a blocked word is matched.
    /// </summary>
    public enum FilterAction
    {
        /// <summary>Replace matched words with a censor string (e.g. asterisks).</summary>
        Censor,

        /// <summary>Block the entire message from being sent.</summary>
        Block,
    }

    /// <summary>
    /// Result of running a message through the filter.
    /// </summary>
    public readonly struct FilterResult
    {
        public readonly bool WasModified;
        public readonly bool WasBlocked;
        public readonly string Text;

        private FilterResult(string text, bool wasModified, bool wasBlocked)
        {
            Text = text;
            WasModified = wasModified;
            WasBlocked = wasBlocked;
        }

        public static FilterResult Unchanged(string text) => new FilterResult(text, false, false);
        public static FilterResult Censored(string text) => new FilterResult(text, true, false);
        public static FilterResult Blocked() => new FilterResult(string.Empty, false, true);
    }

    /// <summary>
    /// Manages a word filter list and applies censoring or blocking to chat messages.
    /// </summary>
    public class ChatFilterManager
    {
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{HushPlugin.ModName}.CFM");
        private readonly HashSet<string> _blockedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _rawPatterns = new HashSet<string>(StringComparer.Ordinal);
        private Regex? _pattern;

        public FilterAction Action { get; set; } = FilterAction.Censor;
        public char CensorChar { get; set; } = '*';
        public bool Enabled { get; set; } = true;

        /// <summary>Total number of active entries (literals + raw patterns).</summary>
        public int Count => _blockedWords.Count + _rawPatterns.Count;

        /// <summary>
        /// Adds a literal word. Automatically wrapped in <c>\b...\b</c> and matched case-insensitively.
        /// Returns true if it was new.
        /// </summary>
        public bool Add(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return false;
            if (!_blockedWords.Add(word.Trim())) return false;
            _log.LogInfo($"Added word: \"{word.Trim()}\"");
            RebuildPattern();
            return true;
        }

        /// <summary>Removes a literal word. Returns true if it existed.</summary>
        public bool Remove(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return false;
            if (!_blockedWords.Remove(word.Trim())) return false;
            _log.LogInfo($"Removed word: \"{word.Trim()}\"");
            RebuildPattern();
            return true;
        }

        /// <summary>
        /// Adds a raw regex pattern. Use inline flags for control:
        /// <list type="bullet">
        /// <item><c>(?i)pattern</c> - case-insensitive</item>
        /// <item><c>\bword\b</c> - word boundary</item>
        /// <item><c>f+u+c+k</c> - substring (no boundary = matches inside words)</item>
        /// </list>
        /// Returns false if the pattern is invalid regex.
        /// </summary>
        public bool AddPattern(string regexPattern)
        {
            if (string.IsNullOrWhiteSpace(regexPattern)) return false;
            string trimmed = regexPattern.Trim();
            try { _ = new Regex(trimmed); }
            catch (ArgumentException ex) { _log.LogWarning($"Rejected invalid regex \"{trimmed}\": {ex.Message}"); return false; }
            if (!_rawPatterns.Add(trimmed)) return false;
            _log.LogInfo($"Added pattern: \"{trimmed}\"");
            RebuildPattern();
            return true;
        }

        /// <summary>Removes a raw regex pattern. Returns true if it existed.</summary>
        public bool RemovePattern(string regexPattern)
        {
            if (string.IsNullOrWhiteSpace(regexPattern)) return false;
            string trimmed = regexPattern.Trim();
            if (!_rawPatterns.Remove(trimmed)) return false;
            _log.LogInfo($"Removed pattern: \"{trimmed}\"");
            RebuildPattern();
            return true;
        }

        /// <summary>Clears all literal words and raw patterns.</summary>
        public void Clear()
        {
            int count = Count;
            _blockedWords.Clear();
            _rawPatterns.Clear();
            _pattern = null;
            _log.LogInfo($"Filter cleared ({count} entries removed).");
        }

        /// <summary>Returns a snapshot of the current literal words.</summary>
        public string[] GetWords()
        {
            var arr = new string[_blockedWords.Count];
            _blockedWords.CopyTo(arr);
            return arr;
        }

        /// <summary>Returns a snapshot of the current raw regex patterns.</summary>
        public string[] GetPatterns()
        {
            var arr = new string[_rawPatterns.Count];
            _rawPatterns.CopyTo(arr);
            return arr;
        }

        /// <summary>
        /// Applies the filter to a message string.
        /// </summary>
        public FilterResult Apply(string message)
        {
            if (!Enabled || string.IsNullOrEmpty(message))
                return FilterResult.Unchanged(message);

            Regex? pattern = _pattern;
            FilterAction action = Action;
            char censorChar = CensorChar;

            if (pattern == null)
                return FilterResult.Unchanged(message);

            if (action == FilterAction.Block)
            {
                if (pattern.IsMatch(message))
                    return FilterResult.Blocked();
                return FilterResult.Unchanged(message);
            }

            // Censor mode: replace each match with censor chars of same length
            string censored = pattern.Replace(message, match =>
                new string(censorChar, match.Value.Length));

            if (string.Equals(censored, message, StringComparison.Ordinal))
                return FilterResult.Unchanged(message);

            return FilterResult.Censored(censored);
        }

        /// <summary>
        /// Saves the current filter configuration to the specified JSON file.
        /// Creates or overwrites the file.
        /// </summary>
        public void Save(string filePath)
        {
            var dto = new FilterConfig
            {
                Words = new List<string>(_blockedWords),
                Patterns = new List<string>(_rawPatterns),
            };
            string json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            if (HushSettings.VerboseLogging) _log.LogInfo($"Saved filter config ({_blockedWords.Count} words, {_rawPatterns.Count} patterns) to: {filePath}");
        }

        /// <summary>
        /// Loads filter configuration from the specified JSON file.
        /// Silently no-ops if the file does not exist or is corrupt.
        /// </summary>
        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                if (HushSettings.VerboseLogging) _log.LogInfo("No filter config file found, starting fresh.");
                return;
            }
            FilterConfig? dto;
            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                dto = JsonConvert.DeserializeObject<FilterConfig>(json);
            }
            catch (Exception ex) { _log.LogWarning($"Failed to parse filter config: {ex.Message}"); return; }
            if (dto == null) return;
            _blockedWords.Clear();
            _rawPatterns.Clear();
            if (dto.Words != null)
                foreach (string w in dto.Words)
                    if (!string.IsNullOrWhiteSpace(w))
                        _blockedWords.Add(w.Trim());
            if (dto.Patterns != null)
                foreach (string p in dto.Patterns)
                {
                    if (string.IsNullOrWhiteSpace(p)) continue;
                    string trimmed = p.Trim();
                    try { _ = new Regex(trimmed); _rawPatterns.Add(trimmed); }
                    catch (ArgumentException) { _log.LogWarning($"Skipped invalid regex in config: \"{trimmed}\""); }
                }
            RebuildPattern();
            if (HushSettings.VerboseLogging) _log.LogInfo($"Loaded filter config: {_blockedWords.Count} words, {_rawPatterns.Count} patterns.");
        }

        private class FilterConfig
        {
            public List<string>? Words { get; set; }
            public List<string>? Patterns { get; set; }
        }

        private void RebuildPattern()
        {
            bool hasWords = _blockedWords.Count > 0;
            bool hasPatterns = _rawPatterns.Count > 0;

            if (!hasWords && !hasPatterns)
            {
                _pattern = null;
                return;
            }

            var sb = new StringBuilder();

            // Literal words: wrap in \b...\b, case-insensitive via RegexOptions
            if (hasWords)
            {
                sb.Append(@"\b(?:");
                bool first = true;
                foreach (string word in _blockedWords)
                {
                    if (!first) sb.Append('|');
                    sb.Append(Regex.Escape(word));
                    first = false;
                }
                sb.Append(@")\b");
            }

            // Raw patterns: wrapped in a non-capturing group, user controls flags via (?i) etc.
            foreach (string raw in _rawPatterns)
            {
                if (sb.Length > 0) sb.Append('|');
                sb.Append("(?:");
                sb.Append(raw);
                sb.Append(')');
            }

            // IgnoreCase only applies to the literal words section;
            // raw patterns use their own inline flags.
            _pattern = new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            if (HushSettings.VerboseLogging) _log.LogInfo($"Pattern rebuilt: {Count} active entries.");
        }
    }
}
