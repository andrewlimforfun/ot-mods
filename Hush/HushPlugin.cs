
using System;
using System.Collections.Concurrent;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Hush.Core;
using Hush.Core.Commands;
using Hush.Patches;
using Alpha;
using Alpha.Core.Util;
using HarmonyLib;
using UnityEngine;


namespace Hush
{
    [BepInPlugin(HushPlugin.ModGUID, HushPlugin.ModName, HushPlugin.ModVersion)]
    public class HushPlugin : BaseUnityPlugin
    {

        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<bool>? ShowCommand { get; private set; }
        public static ConfigEntry<FilterAction>? FilterActionConfig { get; private set; }
        public static ConfigEntry<string>? CensorCharConfig { get; private set; }
        public static ConfigEntry<string>? FilterConfigPathConfig { get; private set; }
        public static ChatFilterManager? FilterManager { get; private set; }
        public static PlayerMuteManager? MuteManager { get; private set; }

        public static string FilterConfigPath =>
            FilterConfigPathConfig?.Value ?? Path.Combine(Paths.ConfigPath, $"{ModGUID}.filter.json");

        public static string MutesConfigPath =>
            Path.Combine(Paths.ConfigPath, $"{ModGUID}.mutes.json");

        private static ManualLogSource? _logger;

        /// <summary>Saves the current filter state to disk. Safe to call from commands.</summary>
        public static void SaveFilter()
        {
            if (FilterManager == null) return;
            try { FilterManager.Save(FilterConfigPath); }
            catch (Exception ex) { _logger?.LogError($"Failed to save filter config: {ex.Message}"); }
        }

        /// <summary>Saves the current mute list to disk. Safe to call from commands.</summary>
        public static void SaveMutes()
        {
            if (MuteManager == null) return;
            try { MuteManager.Save(MutesConfigPath); }
            catch (Exception ex) { _logger?.LogError($"Failed to save mutes: {ex.Message}"); }
        }

        // Thread-safe queue to marshal background-thread work onto the Unity main thread
        private static readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

        /// <summary>Schedules an action to run on the Unity main thread on the next Update tick.</summary>
        public static void RunOnMainThread(Action action)
        {
            if (EnableFeature?.Value == true) _mainThreadQueue.Enqueue(action);
        }

        public const string ModGUID = "com.andrewlin.ontogether.hush";
        public const string ModName = "Hush";
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

            FilterManager = new ChatFilterManager();
            FilterManager.Load(FilterConfigPath);
            if (FilterActionConfig != null)
            {
                FilterManager.Action = FilterActionConfig.Value;
                FilterActionConfig.SettingChanged += (sender, args) =>
                {
                    if (FilterManager != null) FilterManager.Action = FilterActionConfig.Value;
                };
            }
            if (CensorCharConfig != null)
            {
                if (CensorCharConfig.Value.Length == 1) FilterManager.CensorChar = CensorCharConfig.Value[0];
                CensorCharConfig.SettingChanged += (sender, args) =>
                {
                    if (FilterManager != null && CensorCharConfig.Value.Length == 1)
                        FilterManager.CensorChar = CensorCharConfig.Value[0];
                };
            }

            MuteManager = new PlayerMuteManager();
            MuteManager.Load(MutesConfigPath);

            var harmony = new Harmony(ModGUID);
            harmony.PatchAll(typeof(TextChannelManagerPatch));

            AlphaPlugin.CommandManager?.Register(new HushToggleCommand());
            AlphaPlugin.CommandManager?.Register(new HushAddWordCommand());
            AlphaPlugin.CommandManager?.Register(new HushRemoveWordCommand());
            AlphaPlugin.CommandManager?.Register(new HushAddPatternCommand());
            AlphaPlugin.CommandManager?.Register(new HushRemovePatternCommand());
            AlphaPlugin.CommandManager?.Register(new HushGetWordsCommand());
            AlphaPlugin.CommandManager?.Register(new HushGetPatternsCommand());
            AlphaPlugin.CommandManager?.Register(new HushFilterActionCommand());
            AlphaPlugin.CommandManager?.Register(new HushCensorCharCommand());
            AlphaPlugin.CommandManager?.Register(new HushLoadFilterCommand());
            AlphaPlugin.CommandManager?.Register(new HushMuteCommand());
            AlphaPlugin.CommandManager?.Register(new HushUnmuteCommand());
            AlphaPlugin.CommandManager?.Register(new HushTempMuteCommand());
            AlphaPlugin.CommandManager?.Register(new HushGetMutesCommand());
            AlphaPlugin.CommandManager?.Register(new HushDelegateAddCommand());
            AlphaPlugin.CommandManager?.Register(new HushDelegateRemoveCommand());
            AlphaPlugin.CommandManager?.Register(new HushDelegateListCommand());
            AlphaPlugin.CommandManager?.Register(new HushVersionCommand());
        }

        void InitConfig()
        {
            // Initialize config entries
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the mod feature.");
            ShowCommand = Config.Bind("General", "ShowCommand", false, "Show the command in chat when used.");
            FilterActionConfig = Config.Bind("Filter", "Action", FilterAction.Censor, "How the filter handles matched words: Censor (replace with asterisks) or Block (suppress entire message).");
            CensorCharConfig = Config.Bind("Filter", "CensorChar", "*", "Character used to replace matched words when in Censor mode.");
            FilterConfigPathConfig = Config.Bind("Filter", "ConfigPath", Path.Combine(Paths.ConfigPath, $"{ModGUID}.filter.json"), "Path to the filter word list JSON file.");
        }

        /// <summary> Called every frame by Unity. We use it to execute actions on the main thread that were scheduled from background threads (e.g. WebSocket message handlers).</summary>
        void Update()
        {
            if (EnableFeature?.Value != true)
            {
                return;
            }

            MuteManager?.Tick();

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
