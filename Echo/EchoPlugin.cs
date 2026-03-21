
using System;
using System.Collections.Concurrent;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Echo.Core;
using Echo.Core.Commands;
using Alpha;
using Alpha.Core.Util;
using HarmonyLib;
using UnityEngine;


namespace Echo
{
    [BepInPlugin(EchoPlugin.ModGUID, EchoPlugin.ModName, EchoPlugin.ModVersion)]
    public class EchoPlugin : BaseUnityPlugin
    {

        public static ConfigEntry<bool>? EnableFeature { get; private set; }

        private static ManualLogSource? _logger;

        // Thread-safe queue to marshal background-thread work onto the Unity main thread
        private static readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

        /// <summary>Schedules an action to run on the Unity main thread on the next Update tick.</summary>
        public static void RunOnMainThread(Action action)
        {
            if (EnableFeature?.Value == true) _mainThreadQueue.Enqueue(action);
        }

        public const string ModGUID = "com.andrewlin.ontogether.echo";
        public const string ModName = "Echo";
        public const string ModVersion = BuildInfo.Version;

        /// <summary>
        /// Called once when the game starts. Use it to initialize config, set up Harmony patches, initialize resources, etc.
        /// </summary>
        void Awake()
        {
            // This runs once when the game starts
            _logger = Logger;
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");

            InitConfig();

            var harmony = new Harmony(ModGUID);

            AlphaPlugin.CommandManager?.Register(new EchoToggleCommand());
        }

        void InitConfig()
        {
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the mod feature.");
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
        }

        /// <summary> Called when the plugin is unloaded or the game exits. Clean up resources here.</summary>
        void OnDestroy() { }
    }
}
