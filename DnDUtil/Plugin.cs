
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
    [BepInPlugin(Plugin.ModGUID, Plugin.ModName, Plugin.ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<bool>? ShowCommand { get; private set; }
        public static ConfigEntry<string>? AnnouncerArea { get; private set; }
        public static ConfigEntry<string>? AnnouncerChatName { get; private set; }

        public static string? PlayerId { get; private set; }

        public static CommandManager? CommandProcessor { get; private set; }

        public const string DefaultAnnouncerChatName = "DnDSystem";

        public const string ModGUID = "com.andrewlin.ontogether.dndmod";
        public const string ModName = "DnDUtil";
        public const string ModVersion = BuildInfo.Version;

        void Awake()
        {
            // This runs once when the game starts
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");

            // Apply Harmony patches
            var harmony = new Harmony(ModGUID);
            harmony.PatchAll(typeof(TextChannelManagerPatch));

            InitConfig();

            // Initialize command processor with all commands found via reflection
            CommandProcessor = new CommandManager();
        }

        void InitConfig()
        {
            // Initialize config entries
            // config only exists after plugin loads, so cant be in constructor
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the DnD utility features.");
            ShowCommand = Config.Bind("General", "ShowCommand", false, "Show the command in chat when used.");
            
            AnnouncerChatName = Config.Bind("General", "AnnouncerChatName", DefaultAnnouncerChatName, "The chat name to use when announcing rolls. Default is 'DnDSystem'.");
            AnnouncerArea = Config.Bind("General", "AnnouncerArea", "self", "The area to use when announcing rolls [self|local|global]. Default is 'self'.");
        }
    }
}
