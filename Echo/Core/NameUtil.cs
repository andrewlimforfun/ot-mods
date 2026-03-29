using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using BepInEx.Configuration;
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
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Echo.NU");
        const string OfficerBallsStatusManagerGUID = "officerballs.StatusManager";
        /// <summary>The name saved before any Echo rename. Null = no rename active.</summary>
        public static string? OriginalName { get; private set; }

        public static string GetName()
        {
            var dataManager = MonoSingleton<DataManager>.I;
            if (dataManager == null)
            {
                _log.LogWarning("GetName called before DataManager is available.");
                return OriginalName ?? "Player";
            }
            return dataManager.PlayerData.Name;
        }
          
        /// <summary>
        /// Sets the local player's display name and broadcasts it to all clients.
        /// Saves the current name as OriginalName before the first rename (idempotent).
        /// </summary>
        public static void SetName(string newName, bool saveOriginal = true)
        {
            var dataManager = MonoSingleton<DataManager>.I;
            var textChannelManager = NetworkSingleton<TextChannelManager>.I;
            var uiManager = MonoSingleton<UIManager>.I;
            var mainSceneManager = MonoSingleton<MainSceneManager>.I;
            if (dataManager == null || textChannelManager == null)
            {
                _log.LogWarning("SetName called before DataManager/TextChannelManager are available.");
                return;
            }

            if (saveOriginal && OriginalName == null)
            {
                string? baseName = null;
                if (Chainloader.PluginInfos.TryGetValue(OfficerBallsStatusManagerGUID, out var _))
                {
                    // get from officer balls persisted name General.PlayerName
                    var entry = Chainloader.PluginInfos[OfficerBallsStatusManagerGUID].Instance.Config["General", "PlayerName"] as ConfigEntry<string>;
                    baseName = entry?.Value;
                }

                if (!string.IsNullOrEmpty(baseName))
                {
                    OriginalName = baseName;
                }
                else
                {
                    OriginalName = dataManager.PlayerData.Name;
                }
            }
            dataManager.PlayerData.Name = newName;

            if (mainSceneManager != null)
            {
                textChannelManager.MainCustomizationController.UpdatePlayerInfo(dataManager.PlayerData.GetPlayerIdInfo());
                uiManager.PlayerText.text = newName;
                textChannelManager.UserName = newName;

                if (Chainloader.PluginInfos.TryGetValue(OfficerBallsStatusManagerGUID, out var _))
                {
                    ChatUtils.UISendMessage($"/setname {newName}");
                }
            }


            _log.LogInfo($"Name set to: {newName}");
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
