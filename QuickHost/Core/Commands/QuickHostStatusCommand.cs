using Alpha.Core.Command;
using Alpha.Core.Util;

namespace QuickHost.Core.Commands
{
    public class QuickHostStatusCommand : IChatCommand
    {
        public string Name => "quickhoststatus";
        public string ShortName => "qhs";
        public string Description => "Show current QuickHost configuration.";
        public string Namespace => "quickhost";

        public void Execute(string[] args)
        {
            string enabled = QuickHostPlugin.Enabled?.Value == true ? "ON" : "OFF";
            string lobby = QuickHostPlugin.LobbyName?.Value ?? "(not set)";
            int max = QuickHostPlugin.MaxPlayers?.Value ?? 64;
            string vis = QuickHostPlugin.Visibility?.Value ?? "Public";
            bool rtj = QuickHostPlugin.RequestToJoin?.Value ?? false;
            bool triggered = AutoHostService.HasTriggered;

            string tags = string.Join(", ", new[]
            {
                QuickHostPlugin.TagFocus?.Value == true ? "Focus" : null,
                QuickHostPlugin.TagMature?.Value == true ? "Mature" : null,
                QuickHostPlugin.TagChill?.Value == true ? "Chill" : null,
                QuickHostPlugin.TagBreak?.Value == true ? "Break" : null,
                QuickHostPlugin.TagModded?.Value == true ? "Modded" : null
            });
            if (string.IsNullOrEmpty(tags)) tags = "(none)";

            ChatUtils.AddGlobalNotification(
                $"[QuickHost] {enabled} | Lobby: \"{lobby}\" | {vis} | Max: {max} | RTJ: {rtj}\n" +
                $"Tags: {tags} | Triggered this session: {triggered}");
        }
    }
}
