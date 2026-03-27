using Fomo;
using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Fomo.Core.Commands
{
    /// <summary>
    /// Toggles whether messages dispatched from external sources (Telegram, WebSocket, etc.)
    /// are sent as local or global chat. Persisted via BepInEx config.
    /// </summary>
    public class FomoIncomingModeCommand : IChatCommand
    {
        public const string CMD = "fomoincomingmode";
        public string Name => CMD;
        public string ShortName => "fim";
        public string Description =>
            $"Toggle incoming message channel between global and local. " +
            $"Current: {(FomoPlugin.DispatchIncomingLocal?.Value == true ? "local" : "global")}. " +
            $"Usage: /{CMD} [global|local]";

        public string Namespace => "fomo";

        public void Execute(string[] args)
        {
            if (FomoPlugin.DispatchIncomingLocal == null)
                return;

            if (args.Length == 0)
            {
                // Toggle
                FomoPlugin.DispatchIncomingLocal.Value = !FomoPlugin.DispatchIncomingLocal.Value;
            }
            else
            {
                string mode = args[0].ToLower();
                if (mode == "local")
                    FomoPlugin.DispatchIncomingLocal.Value = true;
                else if (mode == "global")
                    FomoPlugin.DispatchIncomingLocal.Value = false;
                else
                {
                    ChatUtils.AddGlobalNotification($"Unknown mode '{args[0]}'. Use 'global' or 'local'.");
                    return;
                }
            }

            string current = FomoPlugin.DispatchIncomingLocal.Value ? "local" : "global";
            ChatUtils.AddGlobalNotification($"Incoming messages will now be sent to {current} chat.");
        }
    }
}
