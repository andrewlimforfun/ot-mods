using System;
using System.Collections.Generic;
using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class RollCommand : IChatCommand
    {
        public string Name => "roll";
        public string Description => "Rolls a dice. E.g. '/roll 2d20 1d6' rolls 2 20-sided dice and 1 6-sided die.";
        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /roll [XdY] - Rolls X dice with Y sides. Example: /roll 2d20");
                return;
            }

            for (int arg = 0; arg < args.Length; arg++)
            {
                // XdY format, e.g. 2d20 where X is number of dice and Y is number of sides
                // X is optional
                string input = args[arg];

                try
                {
                    List<int> rolls = DiceTokenToNumber(input);

                    string result = string.Join(", ", rolls);

                    string userName = ChatUtils.GetUserName();
                    var message = $"{userName} rolled {input}: {result}";
                    string dndChatName = Plugin.AnnouncerChatName?.Value ?? Plugin.DefaultAnnouncerChatName;

                    // pick notification or chat message based on config
                    if (Plugin.AnnouncerArea?.Value == "self")
                    {
                        ChatUtils.AddGlobalNotification(message);
                    }
                    else if (Plugin.AnnouncerArea?.Value == "local")
                    {
                        ChatUtils.SendMessageAsync(dndChatName, message, Islocal: true);
                    }
                    else if (Plugin.AnnouncerArea?.Value == "global")
                    {
                        ChatUtils.SendMessageAsync(dndChatName, message);
                    }
                }
                catch (ArgumentException ex)
                {
                    ChatUtils.AddGlobalNotification(ex.Message);
                    continue;
                }
            }
        }

        public static List<int> DiceTokenToNumber(string token)
        {
            if (!token.Contains('d'))
            {
                throw new ArgumentException($"Invalid format '{token}'. Use XdY, e.g. 2d20.");
            }

            if (token.StartsWith('d'))
            {
                token = "1" + token; // default to 1 die if X is missing, e.g. d6 -> 1d6
            }

            string[] parts = token.Split('d');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int numDice) || !int.TryParse(parts[1], out int sides))
            {
                throw new ArgumentException($"Invalid format '{token}'. Use XdY, e.g. 2d20.");
            }

            var rand = new Random();
            List<int> rolls = new List<int>();
            for (int i = 0; i < numDice; i++)
            {
                rolls.Add(rand.Next(1, sides + 1));
            }

            return rolls;
        }
    }
}
