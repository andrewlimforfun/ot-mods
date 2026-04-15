using System.Text;
using Alpha.Core.Command;
using Alpha.Core.Util;
using Echo.Core;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echolocationlist — lists all saved locations and their coordinates.
    /// </summary>
    public class EchoLocationListCommand : IChatCommand
    {
        public string Name => "echolocationlist";
        public string ShortName => "ell";
        public string Description => "List all saved locations and their coordinates. Usage: /echolocationlist";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            var locations = EchoPlugin.Locations;
            if (locations == null)
            {
                ChatUtils.AddGlobalNotification("Location manager not initialized.");
                return;
            }

            var all = locations.GetAll();
            if (all.Count == 0)
            {
                ChatUtils.AddGlobalNotification("No saved locations.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Saved locations ({all.Count}):");
            foreach (var kv in all)
                sb.AppendLine($"  {kv.Key}: ({kv.Value.x:F1}, {kv.Value.y:F1}, {kv.Value.z:F1})");

            ChatUtils.AddGlobalNotification(sb.ToString().TrimEnd());
        }
    }
}
