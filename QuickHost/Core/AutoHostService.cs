using System;
using System.Collections;
using System.Collections.Generic;
using Alpha.Core.Util;
using BepInEx.Logging;
using UnityEngine;

namespace QuickHost.Core
{
    /// <summary>
    /// Configures MultiplayerManager with plugin settings and triggers lobby creation.
    /// </summary>
    internal static class AutoHostService
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("QuickHost.AutoHost");

        private static bool _hasTriggered;

        /// <summary>Whether auto-host has already fired this session.</summary>
        public static bool HasTriggered => _hasTriggered;

        /// <summary>
        /// Called from the main menu patch. Starts the delayed auto-host coroutine.
        /// </summary>
        public static void TryAutoHost(MonoBehaviour host)
        {
            if (_hasTriggered) return;
            if (QuickHostPlugin.Enabled?.Value != true) return;

            string lobbyName = QuickHostPlugin.LobbyName?.Value ?? "";
            if (string.IsNullOrWhiteSpace(lobbyName))
            {
                Logger.LogInfo("LobbyName is empty - skipping auto-host (configure it first).");
                return;
            }

            _hasTriggered = true;
            float delay = QuickHostPlugin.DelaySeconds?.Value ?? 2f;
            host.StartCoroutine(AutoHostCoroutine(delay));
        }

        private static IEnumerator AutoHostCoroutine(float delay)
        {
            Logger.LogInfo($"Auto-host starting in {delay}s...");
            ChatUtils.AddGlobalNotification($"[QuickHost] Creating lobby in {delay:F0}s...");

            yield return new WaitForSeconds(delay);

            try
            {
                var mm = MonoSingleton<MultiplayerManager>.I;
                var ui = MonoSingleton<MainMenuUIController>.I;

                if (mm == null || ui == null)
                {
                    Logger.LogError("MultiplayerManager or MainMenuUIController not available.");
                    yield break;
                }

                if (mm.IsConnecting)
                {
                    Logger.LogWarning("Already connecting - skipping auto-host.");
                    yield break;
                }

                // Configure lobby settings
                string visibility = QuickHostPlugin.Visibility?.Value ?? "Public";
                mm.FilterTypeValue = visibility.Equals("Private", StringComparison.OrdinalIgnoreCase)
                    ? FilterType.Private
                    : FilterType.Public;

                // No upper clamp - PlayerLimitLift mod allows up to 128
                int maxPlayers = Mathf.Max(QuickHostPlugin.MaxPlayers?.Value ?? 64, 1);
                mm.FilterPlayerValue = maxPlayers - 1;

                mm.FilterIsRequestToJoin = QuickHostPlugin.RequestToJoin?.Value ?? false;

                mm.FilterSocialTags = new List<bool>
                {
                    QuickHostPlugin.TagFocus?.Value ?? true,
                    QuickHostPlugin.TagMature?.Value ?? true,
                    QuickHostPlugin.TagChill?.Value ?? true,
                    QuickHostPlugin.TagBreak?.Value ?? true,
                    QuickHostPlugin.TagModded?.Value ?? true
                };

                // Set lobby name in the input field (CreateLobby reads from it)
                ui.CreateSessionNameInputField.text = QuickHostPlugin.LobbyName!.Value;

                Logger.LogInfo($"Creating lobby \"{QuickHostPlugin.LobbyName.Value}\" (max {maxPlayers}, {visibility})...");
                mm.CreateLobby();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Auto-host failed: {ex}");
            }
        }
    }
}
