using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echoremovestatus <status brackets> - remove status from my player's display name.
    /// </summary>
    public class EchoRemoveStatusCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Echo.ERSC");

        public string Name => "echoremovestatus";
        public string ShortName => "ers";
        public string Description => "Remove status from my player's display name. Usage: /echoremovestatus <status_brackets>";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (EchoPlugin.AccessToken == null || !EchoPlugin.Validator.IsValid(EchoPlugin.AccessToken.Value.Trim()))
            {
                ChatUtils.AddGlobalNotification("Access denied: remove status has high abuse potential. Only Certified Users can use.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /echoremovestatus <status_brackets>");
                return;
            }

            if (args.Length == 2)
            {
                ChatUtils.AddGlobalNotification("Please provide only one argument with the brackets to remove. Usage: /echoremovestatus <status_brackets>");
                return;
            }

            string bracketArg = args[0].Trim();

            string nameWithTags = NameUtil.GetName(); // get current player's name
            if (bracketArg.Length == 1)
            {
                int bracketIndex = nameWithTags.LastIndexOf(bracketArg[0]);
                nameWithTags = nameWithTags[..bracketIndex];
            }
            else if (bracketArg.Length > 1)
            {
                char bracketStart = bracketArg[0];
                char bracketEnd = bracketArg[^1];
                nameWithTags = RemoveLastBracketedSection(nameWithTags, bracketStart, bracketEnd);
            }

            NameUtil.SetName(nameWithTags, saveOriginal: false);
            ChatUtils.AddGlobalNotification($"Status removed from {nameWithTags}");
        }

        public static string RemoveLastBracketedSection(string name, char bracketStart, char bracketEnd)
        {
            int lastEnd = name.LastIndexOf(bracketEnd);
            if (lastEnd == -1) return name;

            int lastStart = name.LastIndexOf(bracketStart, lastEnd);
            if (lastStart == -1) return name;

            return name.Remove(lastStart, lastEnd - lastStart + 1).TrimEnd();
        }
    }
}
