using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echocopyname <name|steamid_suffix|persona> - copy another player's full display name (with TMP tags).
    /// </summary>
    public class EchoCopyNameCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Echo.ECONC");

        public string Name => "echocopyname";
        public string ShortName => "ecn";
        public string Description => "Copy another player's display name (with TMP tags). Usage: /echocopyname <name|steamid_suffix|persona> [status_brackets]";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (EchoPlugin.AccessToken == null || !EchoPlugin.Validator.IsValid(EchoPlugin.AccessToken.Value.Trim()))
            {
                ChatUtils.AddGlobalNotification("Access denied: copy name has high abuse potential. Only Certified Users can use.");
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /echocopyname <name|steamid_suffix|persona> [status_brackets]");
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

            string cleanName = ChatUtils.CleanTMPTags(nameWithTags).Trim();
            _log.LogInfo($"Copying name from {cleanName}: \"{nameWithTags}\"");

            NameUtil.SetName(nameWithTags);
            ChatUtils.AddGlobalNotification($"Name copied from {nameWithTags}");
        }

    }
}
