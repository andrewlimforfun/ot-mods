using System.Collections.Generic;
using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class SetAnnouncerAreaCommand : IChatCommand
    {
        public static readonly HashSet<string> ValidAreas = new HashSet<string>() { "self", "local", "global" };
        public static readonly string ValidAreasText = string.Join("|", ValidAreas);
        public const string CMD = "setannouncerarea";
        public string Name => CMD;
        public string Description => $"Set the area to use when announcing rolls: [{ValidAreasText}]. Current: " + (Plugin.AnnouncerArea?.Value ?? "self");

        
        public void Execute(string[] args)
        {
            if (Plugin.AnnouncerArea == null)
            {
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Usage: /setannouncerarea [{ValidAreasText}] - Sets the area to use when announcing rolls.");
                return;
            }

            string area = args[0].ToLower();
            
            if (!ValidAreas.Contains(area)) 
            {
                ChatUtils.AddGlobalNotification($"Invalid announcer area. Valid areas are: [{ValidAreasText}].");
                return;
            }

            Plugin.AnnouncerArea.Value = area;
            ChatUtils.AddGlobalNotification($"Announcer area is now set to {Plugin.AnnouncerArea.Value}.");
        }
    }
}
