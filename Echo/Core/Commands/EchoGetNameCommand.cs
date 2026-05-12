using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echogetname <name|steamid_suffix|persona> - copy another player's full display name (with TMP tags).
    /// </summary>
    public class EchoGetNameCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Echo.ECONC");

        public string Name => "echogetname";
        public string ShortName => "egn";
        public string Description => "Copy another player's display name (with TMP tags). Usage: /echogetname <name|steamid_suffix|persona> [status_brackets]";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            bool isAdmin = PlayerLists.IsAdmin(SteamUtils.GetPlayerSteamID());
            if (!isAdmin && (EchoPlugin.AccessToken == null || !EchoPlugin.Validator.IsValid(EchoPlugin.AccessToken.Value.Trim())))
            {
                ChatUtils.AddGlobalNotification("Access denied: get name has high abuse potential. Only Certified Users can use.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /echogetname <name|steamid_suffix|persona> [status_brackets]");
                return;
            }

            string query = string.Join(" ", args).Trim();
            PlayerDetail? target = PlayerUtils.FindPlayerByQuery(query);
            if (target == null)
            {
                ChatUtils.AddGlobalNotification($"Player \"{query}\" not found.");
                return;
            }

            string nameWithTags = target.UserName; // raw TMP string from PlayerNameText
            string cleanName = target.UserNameClean; // cleaned name without TMP tags, for logging and notifications
            _log.LogInfo($"Name of {cleanName}: \"{nameWithTags}\"");
            ChatUtils.AddGlobalNotification($"Name of {cleanName}: {nameWithTags}");
        }

    }
}
