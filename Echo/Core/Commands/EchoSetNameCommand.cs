using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using Echo.Core;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echosetname (/esn) - Change your display name text while preserving the current TMP tag styling.
    /// Usage: /esn &lt;new name&gt;
    /// </summary>
    public class EchoSetNameCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.EchoSetNameCommand");

        public string Name => "echosetname";
        public string ShortName => "esn";
        public string Description => "Change name text while keeping current style. Usage: /esn <new name>";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /esn <new name>");
                return;
            }

            string newChars = string.Join(" ", args).Trim();
            if (newChars.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Name cannot be empty.");
                return;
            }

            string currentName = NameUtil.GetName();
            var segments = NameStyleTransfer.ParseSegments(currentName);

            // Check if there are any tags to preserve
            bool hasTags = false;
            foreach (var seg in segments)
            {
                if (seg.Tags.Length > 0)
                {
                    hasTags = true;
                    break;
                }
            }

            string styledName = hasTags
                ? NameStyleTransfer.ApplyEven(segments, newChars)
                : newChars;

            Logger.LogInfo($"Set name to \"{styledName}\" (preserving style).");
            NameUtil.SetName(styledName);
            ChatUtils.AddGlobalNotification($"Name set to \"{ChatUtils.CleanTMPTags(styledName)}\" with existing style.");
        }
    }
}
