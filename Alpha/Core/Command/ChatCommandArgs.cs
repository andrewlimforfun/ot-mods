using System;
using System.Linq;

namespace Alpha.Core.Command
{
    /// <summary>
    /// Represents parsed command arguments from a command string.
    /// </summary>
    public class ChatCommandArgs
    {
        /// <summary>
        /// An empty <see cref="ChatCommandArgs"/> instance.
        /// </summary>
        public static readonly ChatCommandArgs EMPTY = new ChatCommandArgs("", new string[0]);
    
        public string Name { get; }
        public string[] Args { get; }

        public ChatCommandArgs(string name, string[] args)
        {
            Name = name;
            Args = args;
        }

        /// <summary>
        /// Attempts to parse a command string into a <see cref="ChatCommandArgs"/> instance.
        /// </summary>
        /// <param name="input">The input command string (should start with '/').</param>
        /// <param name="result">The parsed <see cref="ChatCommandArgs"/> if successful; otherwise, <see cref="EMPTY"/>.</param>
        /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out ChatCommandArgs result)
        {
            if (string.IsNullOrWhiteSpace(input) || !input.StartsWith('/'))
            {
                result = EMPTY;
                return false;
            }

            // don't lowercase input since some args may be case-sensitive (like names)
            var parts = input.Trim()[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var name = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();
        
            result = new ChatCommandArgs(name, args);
            return true;
        }

        public override string ToString()
        {
            return $"/{Name} {string.Join(' ', Args)}";
        }
    }
}
