using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Alpha.Core.Util;

namespace Alpha.Core.Command
{
    public class ChatCommandManager
    {
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{AlphaPlugin.ModName}.CM");

        private readonly Dictionary<string, IChatCommand> _commands = new Dictionary<string, IChatCommand>();
        private readonly Dictionary<string, List<IChatCommand>> _byNamespace = new Dictionary<string, List<IChatCommand>>();

        public ChatCommandManager() { }

        public void Register(IChatCommand? command)
        {
            if (command == null || string.IsNullOrWhiteSpace(command.Name))
            {
                _log.LogWarning($"Attempted to register null or nameless command: {command?.GetType().Name ?? "null"}");
                return;
            }

            _commands[command.Name] = command;
            if (!string.IsNullOrWhiteSpace(command.ShortName))
                _commands[command.ShortName] = command;

            // track by namespace and auto-create /{ns}help on first command in a namespace
            string ns = command.Namespace ?? "";
            if (!string.IsNullOrWhiteSpace(ns))
            {
                if (!_byNamespace.TryGetValue(ns, out List<IChatCommand>? list))
                {
                    list = new List<IChatCommand>();
                    _byNamespace[ns] = list;

                    var helpCmd = new NamespaceHelpCommand(ns, list);
                    _commands[helpCmd.Name] = helpCmd;
                    if (!string.IsNullOrWhiteSpace(helpCmd.ShortName) && !_commands.ContainsKey(helpCmd.ShortName))
                        _commands[helpCmd.ShortName] = helpCmd;
                    _log.LogInfo($"Auto-registered help: /{helpCmd.Name} (/{helpCmd.ShortName})");
                }
                if (!list.Contains(command))
                    list.Add(command);
            }

            string shortInfo = string.IsNullOrWhiteSpace(command.ShortName) ? "" : $" (/{command.ShortName})";
            _log.LogInfo($"Registered command: /{command.Name}{shortInfo} [{ns}]");
        }

        public bool ProcessInput(string input)
        {
            // Split "/roll 2d20 d7" -> ["roll", "2d20", "d7"]
            if (!ChatCommandArgs.TryParse(input, out ChatCommandArgs? commandArgs))
                return false;

            if (_commands.TryGetValue(commandArgs.Name, out IChatCommand? command))
            {
                _log.LogInfo($"Executing command: {commandArgs}");
                try
                {
                    command.Execute(commandArgs.Args);
                }
                catch (Exception ex)
                {
                    ChatUtils.AddGlobalNotification($"Error executing '{commandArgs}': {ex.Message}");
                }
                return true;
            }

            return false;
        }
    }
}
