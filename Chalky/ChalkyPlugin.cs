
using Alpha;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Chalky.Core.Commands;
using Chalky.Patches;
using HarmonyLib;
using UnityEngine;


namespace Chalky
{
    [BepInPlugin(ChalkyPlugin.ModGUID, ChalkyPlugin.ModName, ChalkyPlugin.ModVersion)]
    public class ChalkyPlugin : BaseUnityPlugin
    {
        public static readonly string DefaultChatLogPath = Path.Combine(BepInEx.Paths.BepInExRootPath, "on_together_chat_log.txt");
        public static int chalkySize = 2;
        public static Color? chalkyColor = null;
        public static ConfigEntry<bool>? EnableFeature { get; private set; }
        public static ConfigEntry<bool>? ShowCommand { get; private set; }
        public static ConfigEntry<bool>? SpoofHost { get; private set; }
        public static ConfigEntry<string>? BoardSaveDirectory { get; private set; }

        /// <summary>Resolved absolute path to the board save directory.</summary>
        public static string SaveDir { get; private set; } = "";

        public static string? PlayerId { get; private set; }


        public const string ModGUID = "com.andrewlin.ontogether.chalky";
        public const string ModName = "Chalky";
        public const string ModVersion = BuildInfo.Version;

        // This method is called when the plugin is loaded
        void Awake()
        {
            // This runs once when the game starts
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");

            InitConfig();

            // Apply Harmony patches
            var harmony = new Harmony(ModGUID);
            harmony.PatchAll(typeof(TextChannelManagerPatch));
            harmony.PatchAll(typeof(QuadPainterGPUPatch));

            // Register commands with the shared Alpha command manager
            AlphaPlugin.CommandManager?.Register(new ChalkyGetBoardsCommand());
            AlphaPlugin.CommandManager?.Register(new ChalkyLoadBoardCommand());
            AlphaPlugin.CommandManager?.Register(new ChalkySaveBoardCommand());
            AlphaPlugin.CommandManager?.Register(new ChalkySpoofHostCommand());
            AlphaPlugin.CommandManager?.Register(new ChalkySetSize());
            AlphaPlugin.CommandManager?.Register(new ChalkyToggleCommand());
        }


        void InitConfig()
        {
            // Initialize config entries
            EnableFeature = Config.Bind("General", "EnableFeature", true, "Enable or disable the mod feature.");
            ShowCommand = Config.Bind("General", "ShowCommand", false, "Show the command in chat when used.");
            SpoofHost = Config.Bind("General", "SpoofHost", false, "If true, spoof rpcInfo in GetQuadImage so sender appears as the host and asServer is true.");
            BoardSaveDirectory = Config.Bind(
                "Boards",
                "BoardSaveDirectory",
                "~/on-together/chalkboard",
                "Directory where chalkboard saves are stored. '~' expands to your home folder.");

            SaveDir = ResolvePath(BoardSaveDirectory.Value);
            Directory.CreateDirectory(SaveDir);
            Logger.LogInfo($"Board save directory: {SaveDir}");
        }

        /// <summary>Expands '~' to the user's home folder and returns the full path.</summary>
        public static string ResolvePath(string path)
        {
            if (path.StartsWith("~"))
                path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)
                       + path.Substring(1);
            return Path.GetFullPath(path);
        }
    }
}
