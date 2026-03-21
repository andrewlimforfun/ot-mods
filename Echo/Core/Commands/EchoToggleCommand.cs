using Echo;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Echo.Core.Commands
{
    public class EchoToggleCommand : IChatCommand
    {
        public const string CMD = "echotoggle";
        public string Name => CMD;
        public string ShortName => "ht";
        public string Description => "Toggle Echo feature on/off. ";

        public string Namespace => "echo";
        public void Execute(string[] args)
        {
            if (EchoPlugin.EnableFeature == null)
            {
                return;
            }

            EchoPlugin.EnableFeature.Value = !EchoPlugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"Echo feature is now {(EchoPlugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
