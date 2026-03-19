using Alpha.Core.Util;
using System.Collections.Generic;
using DnDUtil;
using Alpha.Core.Command;

namespace DnDUtil.Core.Commands
{
    public class DndSetAnnouncerAreaCommand : IChatCommand
    {
        public static readonly HashSet<string> ValidAreas = new HashSet<string>() { "self", "local", "global" };
        public static readonly string ValidAreasText = string.Join("|", ValidAreas);
        public const string CMD = "dndsetannouncerarea";
        public string Name => CMD;
        public string ShortName => "dsaa";
        public string Description => $"Set the area to use when announcing rolls: [{ValidAreasText}]. Current: " + (DndPlugin.AnnouncerArea?.Value ?? "self");

        
        public string Namespace => "dnd";
        public void Execute(string[] args)
        {
            if (DndPlugin.AnnouncerArea == null)
            {
                return;
            }

            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification($"Usage: /dndsetannouncerarea [{ValidAreasText}] - Sets the area to use when announcing rolls.");
                return;
            }

            string area = args[0].ToLower();
            
            if (!ValidAreas.Contains(area)) 
            {
                ChatUtils.AddGlobalNotification($"Invalid announcer area. Valid areas are: [{ValidAreasText}].");
                return;
            }

            DndPlugin.AnnouncerArea.Value = area;
            ChatUtils.AddGlobalNotification($"Announcer area is now set to {DndPlugin.AnnouncerArea.Value}.");
        }
    }
}
