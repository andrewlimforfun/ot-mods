using System.Diagnostics;
using System.IO;
using Alpha.Core.Command;
using Alpha.Core.Util;
using BepInEx.Logging;
using UnityEngine;

namespace QuickHost.Core.Commands
{
    /// <summary>
    /// /quickhostrestart (/qhr) - Launch the configured restart script and quit the game.
    /// </summary>
    public class QuickHostRestartCommand : IChatCommand
    {
        static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("QuickHost.Restart");

        public string Name => "quickhostrestart";
        public string ShortName => "qhr";
        public string Description => "Restart the game using the configured restart script.";
        public string Namespace => "quickhost";

        public void Execute(string[] args)
        {
            string scriptPath = QuickHostPlugin.RestartScriptPath?.Value ?? "";
            if (string.IsNullOrWhiteSpace(scriptPath))
            {
                ChatUtils.AddGlobalNotification("[QuickHost] RestartScriptPath not configured. Set it in the config file.");
                return;
            }

            if (!File.Exists(scriptPath))
            {
                ChatUtils.AddGlobalNotification($"[QuickHost] Script not found: {scriptPath}");
                return;
            }

            Logger.LogInfo($"Launching restart script: {scriptPath}");
            ChatUtils.AddGlobalNotification("[QuickHost] Restarting...");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-ExecutionPolicy Bypass -WindowStyle Hidden -File \"{scriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Failed to launch restart script: {ex.Message}");
                ChatUtils.AddGlobalNotification($"[QuickHost] Failed to launch script: {ex.Message}");
                return;
            }

            Application.Quit();
        }
    }
}
