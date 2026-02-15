using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using DnDUtil.Core.Commands;

namespace DnDUtil.Core
{
    public class CommandManager
    {
        private readonly ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource(Plugin.ModName + ".CommandManager");

        private readonly Dictionary<string, IChatCommand> commands;

        public CommandManager()
        {
            // Use reflection to find all types that implement IChatCommand and are not abstracts
            IEnumerable<Type> commandTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(IChatCommand).IsAssignableFrom(t) 
                         && !t.IsInterface 
                         && !t.IsAbstract);

            // Load all injected commands into a fast-lookup dictionary
            this.commands = new Dictionary<string, IChatCommand>();
            foreach (var cmdType in commandTypes)
            {
                var cmd = Activator.CreateInstance(cmdType) as IChatCommand;
                if (cmd != null)
                {
                    this.commands[cmd.Name] = cmd;
                    this.commands[cmd.ShortName] = cmd;
                    log.LogInfo($"Loaded /{cmd.Name} (/{cmd.ShortName})");
                }
            }

            // Initialize the help command with the full list of commands for dynamic help text
            InitializeHelpCommand(this.commands);
        }

        private static void InitializeHelpCommand(Dictionary<string, IChatCommand> commands)
        {
            if (commands.TryGetValue(HelpCommand.CMD, out IChatCommand? helpCmd))
            {
                (helpCmd as HelpCommand)?.Initialize(commands.Values);
            }
        }

        public bool ProcessInput(string input)
        {
            // Split "/roll 2d20 d7" -> ["roll", "2d20", "d7"]
            if (!CommandArgs.TryParse(input, out CommandArgs? commandArgs))
            {
                // input not processed : not a valid command format
                return false; 
            }

            // Look up command by name and execute if found
            if (commands.TryGetValue(commandArgs.Name, out IChatCommand? command))
            {
                try
                {
                    command.Execute(commandArgs.Args);
                }
                catch (Exception ex)
                {
                    ChatUtils.AddGlobalNotification($"Error executing '{commandArgs}': {ex.Message}");
                }
                // input processed
                return true;
            }
            else
            {
                // input not processed : no matching command found
                return false;
            }
        }
    }
}
