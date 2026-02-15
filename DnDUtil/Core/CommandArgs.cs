using System;
using System.Linq;

namespace DnDUtil.Core
{
    /// <summary>
    /// Represents parsed command arguments from a command string.
    /// </summary>
    public class CommandArgs
    {
        /// <summary>
        /// An empty <see cref="CommandArgs"/> instance.
        /// </summary>
        public static readonly CommandArgs EMPTY = new CommandArgs("", new string[0]);
    
        public string Name { get; }
        public string[] Args { get; }

        public CommandArgs(string name, string[] args)
        {
            Name = name;
            Args = args;
        }

        /// <summary>
        /// Attempts to parse a command string into a <see cref="CommandArgs"/> instance.
        /// </summary>
        /// <param name="input">The input command string (should start with '/').</param>
        /// <param name="result">The parsed <see cref="CommandArgs"/> if successful; otherwise, <see cref="EMPTY"/>.</param>
        /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out CommandArgs result)
        {
            if (string.IsNullOrWhiteSpace(input) || !input.StartsWith('/'))
            {
                result = EMPTY;
                return false;
            }

            var parts = input.ToLower().Trim()[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var name = parts[0];
            var args = parts.Skip(1).ToArray();
        
            result = new CommandArgs(name, args);
            return true;
        }

        public override string ToString()
        {
            return $"/{Name} {string.Join(' ', Args)}";
        }
    }
}
