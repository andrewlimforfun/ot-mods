using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alpha.Core.Util;

namespace Alpha.Core.Command
{
    /// <summary>
    /// Auto-generated help command for a namespace. Registered by <see cref="ChatCommandManager"/>
    /// the first time a command with that namespace is registered.
    /// Responds to <c>/{namespace}help</c> and short form <c>/{ns[0]}h</c>.
    /// </summary>
    internal class NamespaceHelpCommand : IChatCommand
    {
        private readonly string _namespace;
        private readonly List<IChatCommand> _commands;

        public NamespaceHelpCommand(string ns, List<IChatCommand> commands)
        {
            _namespace = ns;
            _commands = commands;
        }

        public string Name => $"{_namespace}help";
        public string ShortName => _namespace.Length >= 1 ? _namespace[0..1] + "h" : "";
        public string Description => $"Lists all {_namespace} commands.";
        public string Namespace => ""; // exempt from namespace tracking to avoid recursion

        public void Execute(string[] args)
        {
            var sorted = new SortedSet<IChatCommand>(_commands);

            bool verbose = args.Length == 1 && (args[0].ToLower() == "verbose" || args[0].ToLower() == "v");
            bool lookup  = args.Length == 1 && !verbose;

            if (args.Length == 0 || verbose)
            {
                var sb = new StringBuilder($"/{_namespace}help - available commands:");
                foreach (var cmd in sorted)
                {
                    if (cmd.IsHidden) continue; // skip hidden commands in general listing

                    string shortPart = string.IsNullOrWhiteSpace(cmd.ShortName) ? "" : $" (/{cmd.ShortName})";
                    string descPart  = verbose ? $": {cmd.Description}" : "";
                    sb.Append($"\n/{cmd.Name}{shortPart}{descPart}");
                }
                ChatUtils.AddGlobalNotification(sb.ToString());
                return;
            }

            // hidden commands can still be looked up directly by name
            if (lookup)
            {
                string needle = args[0].ToLower();
                var cmd = sorted.FirstOrDefault(c => c.Name == needle || c.ShortName == needle);
                if (cmd != null)
                {
                    string shortPart = string.IsNullOrWhiteSpace(cmd.ShortName) ? "" : $" (/{cmd.ShortName})";
                    ChatUtils.AddGlobalNotification($"/{cmd.Name}{shortPart}: {cmd.Description}");
                }
                else
                {
                    ChatUtils.AddGlobalNotification($"Unknown command '{needle}'. Type /{_namespace}help for a list.");
                }
            }
        }
    }
}
