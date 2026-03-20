
using System;
using System.Collections.Concurrent;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Remind.Core;
using Remind.Core.Commands;
using Alpha;
using Alpha.Core.Util;
using HarmonyLib;
using UnityEngine;


namespace Remind
{
    [BepInPlugin(RemindPlugin.ModGUID, RemindPlugin.ModName, RemindPlugin.ModVersion)]
    public class RemindPlugin : BaseUnityPlugin
    {
        public const int OriginalNotificationLimit = 99;
        public const int DefaultNotificationLimit = 300;
        public const int MaxNotificationLimit = 999;
        public const int DefaultChatSinkLocalRange = 5;
        public const int DefaultGlobalChatMessageLimit = 50;
        public const int DefaultLocalChatMessageLimit = 25;
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<bool>? ChatConfirmation { get; private set; }
        public static ConfigEntry<bool>? ShowCommand { get; private set; }
        public static ScheduledTaskManager? ScheduledTaskManager { get; private set; }

        // Thread-safe queue to marshal background-thread work onto the Unity main thread
        private static readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

        /// <summary>Schedules an action to run on the Unity main thread on the next Update tick.</summary>
        public static void RunOnMainThread(Action action)
        {
            if (EnableFeature?.Value == true) _mainThreadQueue.Enqueue(action);
        }

        public const string ModGUID = "com.andrewlin.ontogether.remind";
        public const string ModName = "Remind";
        public const string ModVersion = BuildInfo.Version;

        /// <summary>
        /// Called once when the game starts. Use it to initialize config, set up Harmony patches, initialize resources, etc.
        /// </summary>
        void Awake()
        {
            // This runs once when the game starts
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");

            InitConfig();

            // Apply Harmony patches
            var harmony = new Harmony(ModGUID);
            // harmony.PatchAll(typeof(TextChannelManagerPatch));

            // Register commands with the shared Alpha command manager
            AlphaPlugin.CommandManager?.Register(new RemindChatConfirmationCommand());
            AlphaPlugin.CommandManager?.Register(new RemindGlobalAtCommand());
            AlphaPlugin.CommandManager?.Register(new RemindGlobalInCommand());
            AlphaPlugin.CommandManager?.Register(new RemindLocalAtCommand());
            AlphaPlugin.CommandManager?.Register(new RemindLocalInCommand());
            AlphaPlugin.CommandManager?.Register(new RemindMeAtCommand());
            AlphaPlugin.CommandManager?.Register(new RemindMeInCommand());
            AlphaPlugin.CommandManager?.Register(new RemindToggleCommand());

            // Initialize the task scheduler
            ScheduledTaskManager = new ScheduledTaskManager();
        }

        void InitConfig()
        {
            // Initialize config entries
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the mod feature.");
            ShowCommand = Config.Bind("General", "ShowCommand", false, "Show the command in chat when used.");
            ChatConfirmation = Config.Bind("General", "ChatConfirmation", false, "Broadcast and confirm the creation of reminders in chat.");
        }

        /// <summary> Called every frame by Unity. We use it to execute actions on the main thread that were scheduled from background threads (e.g. WebSocket message handlers).</summary>
        void Update()
        {
            if (EnableFeature?.Value != true)
            {
                return;
            }

            // Drain the main thread queue each frame
            while (_mainThreadQueue.TryDequeue(out Action action))
            {
                try { action(); }
                catch (Exception ex) { Logger.LogError($"Main thread action failed: {ex.Message}"); }
            }

            ScheduledTaskManager?.Tick();
        }

        /// <summary> Called when the plugin is unloaded or the game exits. Clean up resources here.</summary>
        void OnDestroy() { }
    }
}
