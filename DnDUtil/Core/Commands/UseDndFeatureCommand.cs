using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class UseDndFeatureCommand : IChatCommand
    {
        public const string CMD = "usedndfeature";
        public string Name => CMD;
        public string Description => "Toggle DnDUtil feature on/off. ";

        public void Execute(string[] args)
        {
            if (Plugin.EnableFeature == null)
            {
                return;
            }

            Plugin.EnableFeature.Value = !Plugin.EnableFeature.Value;
            ChatUtils.AddGlobalNotification($"DnD feature is now {(Plugin.EnableFeature.Value ? "enabled" : "disabled")}.");
        }
    }
}
