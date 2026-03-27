
using System;
using System.Collections.Concurrent;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Fomo.Core;
using Fomo.Core.Commands;
using Alpha;
using Alpha.Core.Util;
using Fomo.Patches;
using HarmonyLib;
using UnityEngine;


namespace Fomo
{
    [BepInPlugin(FomoPlugin.ModGUID, FomoPlugin.ModName, FomoPlugin.ModVersion)]
    public class FomoPlugin : BaseUnityPlugin
    {
        public const int OriginalNotificationLimit = 99;
        public const int DefaultNotificationLimit = 300;
        public const int MaxNotificationLimit = 999;
        public const int DefaultChatSinkLocalRange = 5;
        public const int DefaultGlobalChatMessageLimit = 50;
        public const int DefaultLocalChatMessageLimit = 25;
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<bool>? ShowCommand { get; private set; }
        public static ConfigEntry<bool>? CleanChatSinkTags { get; private set; }
        public static ConfigEntry<int>? GlobalMessageLimitCount { get; private set; }
        public static ConfigEntry<int>? LocalMessageLimitCount { get; private set; }
        public static ConfigEntry<int>? ChatSinkLocalRange { get; private set; }
        public static ChatSinkManager? SinkManager { get; private set; }

        // Thread-safe queue to marshal background-thread work onto the Unity main thread
        private static readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

        /// <summary>Schedules an action to run on the Unity main thread on the next Update tick.</summary>
        public static void RunOnMainThread(Action action) => _mainThreadQueue.Enqueue(action);

        /// <summary>
        /// Routes an incoming text string (from an external source such as WebSocket or Telegram)
        /// into the appropriate game channel.
        /// </summary>
        public static void DispatchIncomingText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            text = text.Trim();

            if (text.StartsWith("/"))
            {
                RunOnMainThread(() =>
                {
                    if (text.StartsWith("/fomo"))
                    {
                        bool isProcessed = AlphaPlugin.CommandManager?.ProcessInput(text) ?? false;
                        if (isProcessed) return;
                    }
                    ChatUtils.UISendMessage(text);
                });
            }
            else
            {
                string senderName = PlayerUtils.GetUserName();
                ChatUtils.SendMessageAsync(senderName, text, false);
            }
        }

        public const string ModGUID = "com.andrewlin.ontogether.fomo";
        public const string ModName = "Fomo";
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
            harmony.PatchAll(typeof(TextChannelManagerPatch));
            harmony.PatchAll(typeof(GameSettingsPatch));
            harmony.PatchAll(typeof(MainSceneManagerPatch));

            // Register commands with the shared Alpha command manager
            AlphaPlugin.CommandManager?.Register(new FomoChatLocalCommand());
            AlphaPlugin.CommandManager?.Register(new FomoChatGlobalCommand());
            AlphaPlugin.CommandManager?.Register(new FomoChatSinkCleanTagsCommand());
            AlphaPlugin.CommandManager?.Register(new FomoChatSinkGetLocalRangeCommand());
            AlphaPlugin.CommandManager?.Register(new FomoChatSinkSetLocalRangeCommand());
            AlphaPlugin.CommandManager?.Register(new FomoMessageLimitCommand());
            AlphaPlugin.CommandManager?.Register(new FomoToggleCommand());

            // Register output sinks - sub-mods (FomoChatLog, FomoTelegram, etc.) register their own sinks.
            SinkManager = new ChatSinkManager();
        }

        void InitConfig()
        {
            // Initialize config entries
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the mod feature.");
            ShowCommand = Config.Bind("General", "ShowCommand", false, "Show the command in chat when used.");
            CleanChatSinkTags = Config.Bind("General", "CleanChatSinkTags", false, "Strip TMP color/formatting tags (e.g. <#ff0000>) from chat messages before dispatching to sinks (affects all sinks).");
            GlobalMessageLimitCount = Config.Bind("Chat", "GlobalMessageLimitCount", DefaultGlobalChatMessageLimit, "Max number of messages shown in the global chat window. Game default is 50.");
            LocalMessageLimitCount = Config.Bind("Chat", "LocalMessageLimitCount", DefaultLocalChatMessageLimit, "Max number of messages shown in the local chat window. Game default is 25.");
            ChatSinkLocalRange = Config.Bind("Chat", "ChatLogLocalRange", DefaultChatSinkLocalRange, "Local range for chat log messages.");

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

        /// <summary> Called when the plugin is unloaded or the game exits. Clean up resources here.</summary>
        void OnDestroy() { }
    }
}
