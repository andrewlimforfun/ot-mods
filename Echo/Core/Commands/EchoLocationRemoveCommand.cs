using Alpha.Core.Command;
using Alpha.Core.Util;
using Echo.Core;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echolocationremove &lt;name&gt; — removes a saved location by name.
    /// </summary>
    public class EchoLocationRemoveCommand : IChatCommand
    {
        public string Name => "echolocationremove";
        public string ShortName => "elr";
        public string Description => "Remove a saved location. Usage: /echolocationremove <name>";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /echolocationremove <name>");
                return;
            }

            var locations = EchoPlugin.Locations;
            if (locations == null)
            {
                ChatUtils.AddGlobalNotification("Location manager not initialized.");
                return;
            }

            string name = string.Join(" ", args).Trim();
            if (locations.Delete(name))
                ChatUtils.AddGlobalNotification($"Removed location \"{name}\".");
            else
                ChatUtils.AddGlobalNotification($"No saved location named \"{name}\".");
        }
    }
}
