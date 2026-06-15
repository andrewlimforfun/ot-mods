using Alpha.Core.Command;
using Alpha.Core.Util;

namespace Desync.Core.Commands
{
    public class DesyncFixCommand : IChatCommand
    {
        public string Name => "desyncfix";
        public string ShortName => "dsf";
        public string Description => "Toggle a specific fix. Usage: /desyncfix <lobby|host|list|persona> [on|off]";
        public string Namespace => "desync";

        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ChatUtils.AddGlobalNotification(
                    "Usage: /desyncfix <lobby|host|list|persona> [on|off]\n" +
                    $"  lobby  = H1 LobbyRefresh ({On(DesyncPlugin.FixLobbyRefresh)})\n" +
                    $"  host   = H2 HostIdentity ({On(DesyncPlugin.FixHostIdentity)})\n" +
                    $"  list   = H3 PlayerListRepair ({On(DesyncPlugin.FixPlayerListRepair)})\n" +
                    $"  persona= H4 PersonaCache ({On(DesyncPlugin.FixPersonaCache)})");
                return;
            }

            string fixName = args[0].ToLowerInvariant();
            BepInEx.Configuration.ConfigEntry<bool>? entry = fixName switch
            {
                "lobby" => DesyncPlugin.FixLobbyRefresh,
                "host" => DesyncPlugin.FixHostIdentity,
                "list" => DesyncPlugin.FixPlayerListRepair,
                "persona" => DesyncPlugin.FixPersonaCache,
                _ => null
            };

            if (entry == null)
            {
                ChatUtils.AddGlobalNotification($"Unknown fix: {fixName}. Use: lobby, host, list, persona.");
                return;
            }

            if (args.Length >= 2)
            {
                string val = args[1].ToLowerInvariant();
                entry.Value = val == "on" || val == "true" || val == "1";
            }
            else
            {
                entry.Value = !entry.Value;
            }

            ChatUtils.AddGlobalNotification($"Fix '{fixName}' is now {(entry.Value ? "ON" : "OFF")}.");
        }

        static string On(BepInEx.Configuration.ConfigEntry<bool>? entry) =>
            entry?.Value == true ? "ON" : "OFF";
    }
}
