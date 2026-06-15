using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Desync.Core.Commands
{
    public class DesyncToggleCommand : IChatCommand
    {
        public string Name => "desynctoggle";
        public string ShortName => "dst";
        public string Description => "Toggle Desync monitoring on/off.";
        public string Namespace => "desync";

        public void Execute(string[] args)
        {
            if (DesyncPlugin.Enabled == null) return;
            DesyncPlugin.Enabled.Value = !DesyncPlugin.Enabled.Value;
            ChatUtils.AddGlobalNotification($"Desync is now {(DesyncPlugin.Enabled.Value ? "enabled" : "disabled")}.");
        }
    }
}
