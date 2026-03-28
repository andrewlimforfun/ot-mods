using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using PurrNet;
using UnityEngine;
using BepInEx.Bootstrap;

namespace Echo.Core
{
    /// <summary>
    /// Utility for changing the local player's display name and syncing it to all clients.
    /// Stores the original name so it can be reverted.
    /// </summary>
    public static class NameUtil
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Echo.NameUtil");

        /// <summary>The name saved before any Echo rename. Null = no rename active.</summary>
        public static string? OriginalName { get; private set; }

        /// <summary>
        /// Sets the local player's display name and broadcasts it to all clients.
        /// Saves the current name as OriginalName before the first rename (idempotent).
        /// </summary>
        public static void SetName(string newName, bool saveOriginal = true)
        {
            var dm = MonoSingleton<DataManager>.I;
            var tcm = NetworkSingleton<TextChannelManager>.I;
            if (dm == null || tcm == null)
            {
                Logger.LogWarning("SetName called before DataManager/TextChannelManager are available.");
                return;
            }

            if (saveOriginal && OriginalName == null)
                OriginalName = dm.PlayerData.Name;

            dm.PlayerData.Name = newName;

            if (MonoSingleton<MainSceneManager>.I != null)
            {
                tcm.MainCustomizationController.UpdatePlayerInfo(dm.PlayerData.GetPlayerIdInfo());
                MonoSingleton<UIManager>.I.PlayerText.text = newName;
                tcm.UserName = newName;
            }
            
            // set officer balls configBaseName to new name so it shows in the player list and above the head
            if (Chainloader.PluginInfos.TryGetValue("officerballs.StatusManager", out var basicInfo))
            {
                ChatUtils.UISendMessage($"/setname {newName}");
            }

            Logger.LogInfo($"Name set to: {newName}");
        }

        /// <summary>
        /// Reverts to the name saved before any Echo rename. No-op if no rename is active.
        /// </summary>
        public static bool RevertName()
        {
            if (OriginalName == null) return false;
            string revert = OriginalName;
            OriginalName = null; // clear before SetName so it doesn't re-save
            SetName(revert, saveOriginal: false);
            return true;
        }
    }
}
