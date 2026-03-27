using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using PurrNet;

namespace Echo.Core.Commands
{
    public class EchoCopyOutfitSelfCommand : IChatCommand
    {
        static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.ECOSC");

        public string Name => "echocopyoutfitself";
        public string ShortName => "ecos";
        public string Description => "Copy your current outfit/appearance to another slot. Usage: /echocopyoutfit <slot> (1-3)";
        public string Namespace => "echo";

        public void Execute(string[] args)
        {
            if (args.Length != 1 || !int.TryParse(args[0], out int targetSlot1) || targetSlot1 < 1 || targetSlot1 > 3)
            {
                ChatUtils.AddGlobalNotification("Usage: /echocopyoutfit <slot>  (slot = 1, 2, or 3)");
                return;
            }

            int targetIndex = targetSlot1 - 1; // convert to 0-based

            var dm = MonoSingleton<DataManager>.I;
            if (dm == null)
            {
                ChatUtils.AddGlobalNotification("DataManager not available.");
                return;
            }

            var pdz = dm.PlayerDataZip;
            int currentIndex = pdz.SelectedStyleIndex;

            if (targetIndex == currentIndex)
            {
                ChatUtils.AddGlobalNotification($"Slot {targetSlot1} is already your active slot — nothing to copy.");
                return;
            }

            var sourceData = pdz.CurrentCustomizationDataIDs;

            // Temporarily switch to the target slot to use the CurrentCustomizationDataIDs setter,
            // then restore the original slot — avoids depending on individual slot field names
            // which differ between NuGet stub and decompiled game source.
            pdz.SelectedStyleIndex = targetIndex;
            pdz.CurrentCustomizationDataIDs = sourceData;
            pdz.SelectedStyleIndex = currentIndex;

            dm.SavePlayerZipData();

            string sourceName = GetSlotName(pdz, currentIndex);
            string targetName = GetSlotName(pdz, targetIndex);
            Logger.LogInfo($"Copied outfit from slot {currentIndex + 1} ({sourceName}) to slot {targetSlot1} ({targetName}).");
            ChatUtils.AddGlobalNotification($"Copied outfit from slot {currentIndex + 1} ({sourceName}) → slot {targetSlot1} ({targetName}).");
        }

        private static string GetSlotName(PlayerDataZip pdz, int index)
        {
            if (pdz.StyleNames != null && index < pdz.StyleNames.Count)
                return pdz.StyleNames[index];
            return $"Slot {index + 1}";
        }
    }
}
