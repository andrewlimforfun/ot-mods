
using System;
using System.Collections.Concurrent;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Echo.Core;
using Echo.Core.Commands;
using Echo.Patches;
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
        public static ConfigEntry<string>? AccessToken { get; private set; }

        // SHA-256 of the accepted token. Replace with the hash of your actual secret.
        // PowerShell: [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes("yourtoken"))).Replace("-","").ToLower()
        public static readonly TokenValidator Validator = new TokenValidator(
            "c7b7df644ec48a2ce66a51e05d084e177b0a01e6ecb77f9bd91808afe6668148");

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
            harmony.PatchAll(typeof(PlayerMovementControllerPatch));

            AlphaPlugin.CommandManager?.Register(new EchoToggleCommand());
            AlphaPlugin.CommandManager?.Register(new EchoTpToCommand());
            AlphaPlugin.CommandManager?.Register(new EchoCopyNameCommand());
            AlphaPlugin.CommandManager?.Register(new EchoRevertNameCommand());
            AlphaPlugin.CommandManager?.Register(new EchoCopyOutfitCommand());
            AlphaPlugin.CommandManager?.Register(new EchoRemoveStatusCommand());
        }

        void InitConfig()
        {
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the mod feature.");
            AccessToken = Config.Bind("Security", "AccessToken", "put_the_secret_token_here", "Token required to use name/outfit copy commands. Must match the secret embedded in the mod.");
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
