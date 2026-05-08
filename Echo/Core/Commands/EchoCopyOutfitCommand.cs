using System.Reflection;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using PurrNet;

namespace Echo.Core.Commands
{
    /// <summary>
    /// /echocopyoutfit <player> - copy another player's current outfit/appearance.
    /// </summary>
    public class EchoCopyOutfitCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.ECOC");
        static readonly FieldInfo? CustomizationDataField =
            typeof(PlayerCustomizationController).GetField("_customizationData",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public string Name => "echocopyoutfit";
        public string ShortName => "eco";
        public string Description => "Copy another player's current outfit. Usage: /echocopyoutfit <name|steamid_suffix|persona>";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ChatUtils.AddGlobalNotification("Usage: /echocopyoutfit <name|steamid_suffix|persona>");
                return;
            }

            string query = string.Join(" ", args).Trim();
            PlayerDetail? target = PlayerUtils.FindPlayerByQuery(query);

            if (target == null)
            {
                ChatUtils.AddGlobalNotification($"Player \"{query}\" not found.");
                return;
            }

            if (CustomizationDataField == null)
            {
                Logger.LogError("Could not reflect _customizationData on PlayerCustomizationController.");
                ChatUtils.AddGlobalNotification("Reflection error: outfit field not found.");
                return;
            }

            var custCtrl = target.PlayerTransform.GetComponent<PlayerCustomizationController>();
            if (custCtrl == null)
            {
                ChatUtils.AddGlobalNotification($"Could not find customization controller for {ChatUtils.CleanTMPTags(target.UserName).Trim()}.");
                return;
            }

            var data = (CustomizationData)CustomizationDataField.GetValue(custCtrl);

            var localCtrl = NetworkSingleton<TextChannelManager>.I?.MainCustomizationController;
            if (localCtrl == null)
            {
                ChatUtils.AddGlobalNotification("Local customization controller not available.");
                return;
            }

            // ApplyCustomization broadcasts DataManager.CustomizationData (not its argument) via
            // ObserversRpc, so we must update DataManager first or other players see the old outfit.
            MonoSingleton<DataManager>.I.CustomizationData = new CustomizationData(data);
            MonoSingleton<DataManager>.I.PlayerDataZip.CurrentCustomizationDataIDs =
                new CustomizationDataIDs3(MonoSingleton<DataManager>.I.CustomizationData);

            localCtrl.ApplyCustomization(data);
            //MonoSingleton<DataManager>.I.SavePlayerZipData();

            string cleanName = ChatUtils.CleanTMPTags(target.UserName).Trim();
            Logger.LogInfo($"Copied outfit from {cleanName}.");
            ChatUtils.AddGlobalNotification($"Outfit copied from {cleanName}.");
        }
    }
}
