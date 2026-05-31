using System;
using System.Collections.Concurrent;
using Alpha;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using QuickHost.Core;
using QuickHost.Core.Commands;
using QuickHost.Patches;
using HarmonyLib;

namespace QuickHost
{
    [BepInPlugin(QuickHostPlugin.ModGUID, QuickHostPlugin.ModName, QuickHostPlugin.ModVersion)]
    public class QuickHostPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.andrewlin.ontogether.quickhost";
        public const string ModName = "QuickHost";
        public const string ModVersion = BuildInfo.Version;

        internal static ManualLogSource Log = null!;

        // --- Config entries ---
        public static ConfigEntry<bool>? Enabled { get; private set; }
        public static ConfigEntry<string>? LobbyName { get; private set; }
        public static ConfigEntry<int>? MaxPlayers { get; private set; }
        public static ConfigEntry<string>? Visibility { get; private set; }
        public static ConfigEntry<bool>? RequestToJoin { get; private set; }
        public static ConfigEntry<bool>? TagFocus { get; private set; }
        public static ConfigEntry<bool>? TagMature { get; private set; }
        public static ConfigEntry<bool>? TagChill { get; private set; }
        public static ConfigEntry<bool>? TagBreak { get; private set; }
        public static ConfigEntry<bool>? TagModded { get; private set; }
        public static ConfigEntry<float>? DelaySeconds { get; private set; }
        public static ConfigEntry<string>? RestartScriptPath { get; private set; }

        private static readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();
        public static void RunOnMainThread(Action action) => _mainThreadQueue.Enqueue(action);

        void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");
            InitConfig();

            var harmony = new Harmony(ModGUID);
            harmony.PatchAll(typeof(MainMenuUIControllerPatch));

            AlphaPlugin.CommandManager?.Register(new QuickHostToggleCommand());
            AlphaPlugin.CommandManager?.Register(new QuickHostStatusCommand());
            AlphaPlugin.CommandManager?.Register(new QuickHostRestartCommand());
        }

        void InitConfig()
        {
            Enabled = Config.Bind("General", "Enabled", true,
                "When enabled, automatically creates a lobby on game launch.");
            DelaySeconds = Config.Bind("General", "DelaySeconds", 2f,
                "Seconds to wait after main menu loads before creating the lobby.");

            LobbyName = Config.Bind("Lobby", "LobbyName", "",
                "The lobby/session name. Leave empty to skip auto-host (safety for first run).");
            MaxPlayers = Config.Bind("Lobby", "MaxPlayers", 64,
                "Maximum player count for the lobby (1-64).");
            Visibility = Config.Bind("Lobby", "Visibility", "Public",
                "Lobby visibility: Public or Private.");
            RequestToJoin = Config.Bind("Lobby", "RequestToJoin", false,
                "Whether players must request to join.");

            TagFocus = Config.Bind("SocialTags", "Focus", true, "Focus social tag.");
            TagMature = Config.Bind("SocialTags", "Mature", true, "Mature social tag.");
            TagChill = Config.Bind("SocialTags", "Chill", true, "Chill social tag.");
            TagBreak = Config.Bind("SocialTags", "Break", true, "Break social tag.");
            TagModded = Config.Bind("SocialTags", "Modded", true, "Modded social tag.");

            RestartScriptPath = Config.Bind("Maintenance", "RestartScriptPath", "",
                "Absolute path to the restart script (e.g. C:\\scripts\\restart-ontogether.ps1). Leave empty to disable /qhr.");
        }

        void Update()
        {
            while (_mainThreadQueue.TryDequeue(out Action action))
            {
                try { action(); }
                catch (Exception ex) { Log.LogError($"Main thread action failed: {ex.Message}"); }
            }
        }
    }
}
