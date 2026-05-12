using System;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Core;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echocopynamestyle (/ecns) - Keep your name's characters but copy another player's TMP tag styling.
    /// Modes:
    ///   positional (default) - map characters 1:1 by position; overflow goes into last segment.
    ///   even (-e flag)       - distribute characters evenly across tag segments.
    /// Usage: /ecns [-e] &lt;player&gt;
    /// </summary>
    public class EchoCopyNameStyleCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.ECNSC");

        public string Name => "echocopynamestyle";
        public string ShortName => "ecns";
        public string Description => "Copy another player's name styling onto your name. Usage: /ecns [-e] <player>. -e for even distribution.";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            bool isAdmin = PlayerLists.IsAdmin(SteamUtils.GetPlayerSteamID());
            if (!isAdmin && (EchoPlugin.AccessToken == null || !EchoPlugin.Validator.IsValid(EchoPlugin.AccessToken.Value.Trim())))
            {
                ChatUtils.AddGlobalNotification("Access denied: copy name style has high abuse potential. Only Certified Users can use.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /ecns [-e] <player>. -e for even distribution.");
                return;
            }

            bool evenMode = false;
            int queryStart = 0;
            if (args[0].Equals("-e", StringComparison.OrdinalIgnoreCase))
            {
                evenMode = true;
                queryStart = 1;
            }

            if (queryStart >= args.Length)
            {
                ChatUtils.AddGlobalNotification("Usage: /ecns [-e] <player>");
                return;
            }

            string query = string.Join(" ", args, queryStart, args.Length - queryStart).Trim();
            PlayerDetail? target = PlayerUtils.FindPlayerByQuery(query);
            if (target == null)
            {
                ChatUtils.AddGlobalNotification($"Player \"{query}\" not found.");
                return;
            }

            string targetName = target.UserName;
            string myChars = ChatUtils.CleanTMPTags(NameUtil.GetName()).Trim();

            if (myChars.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Your name is empty after cleaning tags.");
                return;
            }

            var segments = NameStyleTransfer.ParseSegments(targetName);

            string styledName = evenMode
                ? NameStyleTransfer.ApplyEven(segments, myChars)
                : NameStyleTransfer.ApplyPositional(segments, myChars);

            Logger.LogInfo($"Style copied from {target.UserNameClean}: \"{styledName}\"");
            NameUtil.SetName(styledName);
            ChatUtils.AddGlobalNotification($"Name style copied from {target.UserNameClean} ({(evenMode ? "even" : "positional")}).");
        }
    }
}
