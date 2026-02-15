
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DnDUtil.Core;
using DnDUtil.Core.Commands;
using DnDUtil.Patches;
using HarmonyLib;
using UnityEngine;


namespace DnDUtil
{
    [BepInPlugin(Plugin.ModGUID, Plugin.ModName, BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<bool>? ShowCommand { get; private set; }
        public static ConfigEntry<bool>? LocalChat { get; private set; }
        public static ConfigEntry<string>? ChatName { get; private set; }

        public static string? PlayerId { get; private set; }

        public static CommandManager? CommandProcessor { get; private set; }

        public const string DefaultChatName = "DnDSystem";

        public const string ModGUID = "com.andrewlin.ontogether.dndmod";
        public const string ModName = "DnDUtil";

        void Awake()
        {
            // This runs once when the game starts
            Logger.LogInfo($"{ModName} v{BuildInfo.Version} is loaded!");

            // Apply Harmony patches
            var harmony = new Harmony(ModGUID);
            harmony.PatchAll(typeof(TextChannelManagerPatch));

            // Initialize config entries
            // config only exists after plugin loads, so cant be in constructor

            EnableFeature = Config.Bind(
                "General",           // Section name
                "EnableFeature",     // Key name
                true,                // Default value
                "Whether to enable the feature"  // Description
            );

            ShowCommand = Config.Bind(
                "General",           // Section name
                "ShowCommand",       // Key name
                false,                // Default value
                "Whether to show the typed commands in chat"  // Description
            );

            LocalChat = Config.Bind(
                "General",           // Section name
                "LocalRollChat",     // Key name
                true,                // Default value
                "Whether to show the roll results only to the local player"  // Description
            );

            ChatName = Config.Bind(
                "General",           // Section name
                "ChatName",         // Key name
                DefaultChatName,    // Default value
                "The name to use when sending messages to chat"  // Description
            );

            // get player id via reflection since TextChannelManager doesn't expose it and we need it to send messages
            // alternatively get player id via Steamworks.SteamUser.GetSteamID().ToString()
            TextChannelManager textChannelManager = NetworkSingleton<TextChannelManager>.I;
            PlayerId = Traverse.Create(textChannelManager).Field<string>("_playerId").Value;
            Logger.LogInfo($"Player ID: {PlayerId}");

            // Initialize command processor with all commands
            CommandProcessor = new CommandManager();

        }
    }
}
