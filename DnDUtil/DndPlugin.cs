
using Alpha;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DnDUtil.Core.Commands;
using HarmonyLib;
using UnityEngine;


namespace DnDUtil
{
    [BepInPlugin(DndPlugin.ModGUID, DndPlugin.ModName, DndPlugin.ModVersion)]
    public class DndPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<bool>? ShowCommand { get; private set; }
        public static ConfigEntry<string>? AnnouncerArea { get; private set; }
        public static ConfigEntry<string>? AnnouncerChatName { get; private set; }

        public static string? PlayerId { get; private set; }


        public const string DefaultAnnouncerChatName = "DnDSystem";

        public const string ModGUID = "com.andrewlin.ontogether.dndmod";
        public const string ModName = "DnDUtil";
        public const string ModVersion = BuildInfo.Version;

        void Awake()
        {
            // This runs once when the game starts
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");

            InitConfig();

            // no patching needed, commands are handled by commons command manager

            // Register commands with the shared Alpha command manager
            AlphaPlugin.CommandManager?.Register(new DndRollCommand());
            AlphaPlugin.CommandManager?.Register(new RollCommand());
            AlphaPlugin.CommandManager?.Register(new DndSetAnnouncerAreaCommand());
            AlphaPlugin.CommandManager?.Register(new DndSetAnnouncerNameCommand());
            AlphaPlugin.CommandManager?.Register(new DndShowCommand());
            AlphaPlugin.CommandManager?.Register(new DndToggleCommand());
        }

        void InitConfig()
        {
            // Initialize config entries
            // config only exists after plugin loads, so cant be in constructor
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the DnD utility features.");
            ShowCommand = Config.Bind("General", "ShowCommand", false, "Show the command in chat when used.");
            
            AnnouncerChatName = Config.Bind("General", "AnnouncerChatName", DefaultAnnouncerChatName, "The chat name to use when announcing rolls. Default is 'DnDSystem'.");
            AnnouncerArea = Config.Bind("General", "AnnouncerArea", "local", "The area to use when announcing rolls [self|local|global]. Default is 'self'.");
        }
    }
}
