using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echorevertname - revert to your original name saved before any /echocopyname.
    /// </summary>
    public class EchoRevertNameCommand : IChatCommand
    {
        public string Name => "echorevertname";
        public string ShortName => "ern";
        public string Description => "Revert your display name to the original before any Echo rename.";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (NameUtil.OriginalName == null)
            {
                ChatUtils.AddGlobalNotification("No saved name to revert to.");
                return;
            }

            string previous = NameUtil.OriginalName;
            NameUtil.RevertName();
            string cleanPrevious = ChatUtils.CleanTMPTags(previous).Trim();
            ChatUtils.AddGlobalNotification($"Name reverted to: {cleanPrevious}");
        }
    }
}
