
using System;
using System.Collections.Concurrent;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Alpha.Core.Command;
using Alpha.Core.Util;
using Alpha.Patches;
using HarmonyLib;
using UnityEngine;
using Alpha.Core.Commands;


namespace Alpha
{
    [BepInPlugin(AlphaPlugin.ModGUID, AlphaPlugin.ModName, AlphaPlugin.ModVersion)]
    public class AlphaPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<bool>? ShowCommand { get; private set; }
        public static ConfigEntry<bool>? TimestampLog { get; private set; }
        public static ChatCommandManager? CommandManager { get; private set; }

        // Thread-safe queue to marshal background-thread work onto the Unity main thread
        private static readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

        /// <summary>Schedules an action to run on the Unity main thread on the next Update tick.</summary>
        public static void RunOnMainThread(Action action) => _mainThreadQueue.Enqueue(action);

        public const string ModGUID = "com.andrewlin.ontogether.alpha";
        public const string ModName = "Alpha";
        public const string ModVersion = BuildInfo.Version;

        /// <summary>
        /// Called once when the game starts. Use it to initialize config, set up Harmony patches, initialize resources, etc.
        /// </summary>
        void Awake()
        {
            InitConfig();

            // Replace default BepInEx disk log listener with a timestamped version
            if (TimestampLog!.Value)
                TimestampLogListener.Install();

            // This runs once when the game starts
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");

            // Apply Harmony patches
            var harmony = new Harmony(ModGUID);
            harmony.PatchAll(typeof(TextChannelManagerPatch));

            // Shared command manager - consuming mods register their commands in their own Awake()
            CommandManager = new ChatCommandManager();
            CommandManager.Register(new AlphaMyPositionCommand());
            CommandManager.Register(new AlphaWhoIsCommand());
            CommandManager.Register(new AlphaServerInfoCommand());
            CommandManager.Register(new AlphaAddNotificationCommand());
            CommandManager.Register(new AlphaUnloadUnusedAssetsCommand());
        }

        void InitConfig()
        {
            // Initialize config entries
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the mod feature.");
            ShowCommand = Config.Bind("General", "ShowCommand", false, "Show the command in chat when used.");
            TimestampLog = Config.Bind("Logging", "TimestampLog", true, "Prepend [HH:mm:ss] timestamps to each line in LogOutput.log.");
        }

        /// <summary> Called every frame by Unity. We use it to execute actions on the main thread that were scheduled from background threads (e.g. WebSocket message handlers).</summary>
        void Update()
        {
            // Drain the main thread queue each frame
            while (_mainThreadQueue.TryDequeue(out Action action))
            {
                try { action(); }
                catch (Exception ex) { Logger.LogError($"Main thread action failed: {ex.Message}"); }
            }
        }

        /// <summary>Called after Awake() on all objects. Steamworks is initialized by this point.</summary>
        void Start() { }

        /// <summary> Called when the plugin is unloaded or the game exits. Clean up resources here.</summary>
        void OnDestroy() { }
    }
}
