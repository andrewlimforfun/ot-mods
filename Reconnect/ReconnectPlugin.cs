using System;
using System.Collections;
using System.Collections.Concurrent;
using Alpha;
using Alpha.Core.Util;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PurrNet;
using PurrNet.Transports;
using Reconnect.Core;
using Reconnect.Core.Commands;
using Reconnect.Patches;
using UnityEngine;

namespace Reconnect
{
    [BepInPlugin(ReconnectPlugin.ModGUID, ReconnectPlugin.ModName, ReconnectPlugin.ModVersion)]
    public class ReconnectPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.andrewlin.ontogether.reconnect";
        public const string ModName = "Reconnect";
        public const string ModVersion = BuildInfo.Version;

        internal static ManualLogSource Log = null!;

        public static ConfigEntry<bool>? Enabled { get; private set; }
        public static ConfigEntry<int>? MaxAttempts { get; private set; }
        public static ConfigEntry<float>? AttemptIntervalSec { get; private set; }
        public static ConfigEntry<float>? CooldownSec { get; private set; }
        public static ConfigEntry<bool>? FixFocusArea { get; private set; }
        public static ConfigEntry<bool>? RestoreFocus { get; private set; }

        internal static ReconnectManager ReconnectManager { get; private set; } = new ReconnectManager(
            () => MaxAttempts?.Value ?? 10,
            () => AttemptIntervalSec?.Value ?? 5f,
            () => CooldownSec?.Value ?? 30f
        );

        /// <summary>
        /// Set to true by <see cref="MultiplayerManagerPatch"/> before a deliberate leave.
        /// Checked in <see cref="MainSceneManagerPatch"/> to decide whether to reconnect.
        /// Reset on successful reconnection or after the reconnect window closes.
        /// </summary>
        public static bool IsIntentionalLeave
        {
            get => ReconnectManager.IsIntentionalLeave;
            set => ReconnectManager.IsIntentionalLeave = value;
        }

        /// <summary>True while a reconnect coroutine is actively running.</summary>
        public static bool IsReconnecting => ReconnectManager.IsReconnecting;

        /// <summary>Saved lobby ID captured at disconnect time for potential full rejoin.</summary>
        public static string? SavedLobbyId
        {
            get => ReconnectManager.SavedLobbyId;
            set => ReconnectManager.SavedLobbyId = value;
        }

        private static ReconnectPlugin? _instance;

        void Awake()
        {
            _instance = this;
            Log = Logger;
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");
            InitConfig();

            var harmony = new Harmony(ModGUID);
            harmony.PatchAll(typeof(MainSceneManagerPatch));
            harmony.PatchAll(typeof(MultiplayerManagerPatch));
            harmony.PatchAll(typeof(PlayerControllerPatch));

            AlphaPlugin.CommandManager?.Register(new ReconnectToggleCommand());
            AlphaPlugin.CommandManager?.Register(new ReconnectMaxAttemptsCommand());
            AlphaPlugin.CommandManager?.Register(new ReconnectIntervalCommand());
            AlphaPlugin.CommandManager?.Register(new ReconnectCooldownCommand());
            AlphaPlugin.CommandManager?.Register(new ReconnectSimulateCommand());
            AlphaPlugin.CommandManager?.Register(new ReconnectFocusCommand());
        }

        void InitConfig()
        {
            Enabled = Config.Bind("General", "Enabled", true,
                "Enable auto-reconnect on unexpected disconnection.");
            MaxAttempts = Config.Bind("General", "MaxAttempts", 100,
                "Maximum reconnect attempts before giving up (1-100).");
            AttemptIntervalSec = Config.Bind("General", "AttemptIntervalSec", 6f,
                "Seconds between reconnect attempts.");
            CooldownSec = Config.Bind("General", "CooldownSec", 30f,
                "Minimum seconds between reconnect sequences to prevent rapid-fire loops.");

            FixFocusArea = Config.Bind("Fix", "FocusArea", true,
                "Re-request focus area state after spawn if Init RPC was missed.");

            RestoreFocus = Config.Bind("Fix", "RestoreFocus", true,
                "Automatically restore focus activity after reconnect.");
        }

        /// <summary>Starts the reconnect coroutine on the plugin MonoBehaviour.</summary>
        internal static Coroutine? StartReconnectCoroutine()
        {
            if (_instance == null) return null;
            return _instance.StartCoroutine(ReconnectCoroutine());
        }

        /// <summary>Starts an arbitrary coroutine on the plugin MonoBehaviour.</summary>
        internal static Coroutine? StartPluginCoroutine(IEnumerator routine)
        {
            if (_instance == null) return null;
            return _instance.StartCoroutine(routine);
        }

        private static IEnumerator ReconnectCoroutine()
        {
            if (!ReconnectManager.TryBeginSequence(Time.unscaledTime))
            {
                Log.LogWarning($"Reconnect cooldown active ({ReconnectManager.CooldownSec}s). Allowing normal disconnect flow.");
                FallbackToMenu();
                yield break;
            }

            Log.LogInfo($"Starting reconnect sequence. Max attempts: {ReconnectManager.MaxAttempts}, interval: {ReconnectManager.AttemptIntervalSec}s");

            while (ReconnectManager.TryNextAttempt())
            {
                ConnectionState state = NetworkManager.main.clientState;
                if (state == ConnectionState.Connected)
                {
                    Log.LogInfo("Already connected - reconnect succeeded (or wasn't needed).");
                    ReconnectManager.OnConnected();
                    yield break;
                }

                ChatUtils.AddGlobalNotification($"Reconnect attempt {ReconnectManager.CurrentAttempt}/{ReconnectManager.MaxAttempts}...");
                Log.LogInfo($"Reconnect attempt {ReconnectManager.CurrentAttempt}/{ReconnectManager.MaxAttempts}...");

                try
                {
                    NetworkManager.main.StartClient();
                }
                catch (Exception ex)
                {
                    Log.LogError($"StartClient() threw: {ex.Message}");
                    break;
                }

                // Wait for the connection attempt to resolve
                float waited = 0f;
                float timeout = ReconnectManager.AttemptIntervalSec;
                while (waited < timeout)
                {
                    yield return null;
                    waited += Time.unscaledDeltaTime;

                    ConnectionState current = NetworkManager.main.clientState;
                    if (current == ConnectionState.Connected)
                    {
                        ChatUtils.AddGlobalNotification($"Reconnected successfully on attempt {ReconnectManager.CurrentAttempt}/{ReconnectManager.MaxAttempts}!");
                        Log.LogInfo($"Reconnected successfully on attempt {ReconnectManager.CurrentAttempt}/{ReconnectManager.MaxAttempts}!");
                        ReconnectManager.OnConnected();
                        yield break;
                    }

                    // If we've settled back to Disconnected, no point waiting longer
                    if (current == ConnectionState.Disconnected && waited > 2f)
                        break;
                }
            }

            // All attempts exhausted - fall back to normal menu return
            Log.LogWarning("Reconnect failed after all attempts. Returning to menu.");
            ReconnectManager.OnFailed();
            FallbackToMenu();
        }

        /// <summary>
        /// Invokes the original disconnect-to-menu flow that was suppressed.
        /// </summary>
        private static void FallbackToMenu()
        {
            // Discard saved state - we're going to menu, not reconnecting
            ReconnectManager.SavedState = null;

            try
            {
                MainSceneManager? msm = MonoSingleton<MainSceneManager>.I;
                if (msm == null) return;

                // Reset _returnMenuStarted so ReturnMenu can proceed
                AccessTools.Field(typeof(MainSceneManager), "_returnMenuStarted")?.SetValue(msm, false);

                MultiplayerManager? mm = MonoSingleton<MultiplayerManager>.I;
                if (mm != null)
                    mm._notificationState = NotificationStatus.HostLost;

                msm.ReturnMenu(false);
            }
            catch (Exception ex)
            {
                Log.LogError($"FallbackToMenu failed: {ex}");
            }
        }

        void OnDestroy()
        {
            _instance = null;
        }
    }
}
