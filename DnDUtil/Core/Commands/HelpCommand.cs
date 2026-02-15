using System;
using System.Collections.Generic;

namespace DnDUtil.Core.Commands
{
    public class HelpCommand : IChatCommand
    {
        public const string CMD = "help";

        public string Name => CMD;
        public string Description => "Lists all available commands.";

        IEnumerable<IChatCommand>? commands;

        public void Initialize(IEnumerable<IChatCommand> values)
        {
            this.commands = values;
        }

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("/help dnd for DnD commands");
            }
            else if (args[0] == "dnd")
            {
                if (commands == null)
                {
                    ChatUtils.AddGlobalNotification("No commands available!");
                    return;
                }

                // list all commands and descriptions
                foreach (var cmd in commands)
                {
                    ChatUtils.AddGlobalNotification($"/{cmd.Name} :: {cmd.Description}");
                }
            }
        }

    }
}
