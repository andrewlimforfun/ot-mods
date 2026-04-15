using System.Collections.Generic;
using BepInEx.Logging;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Compile-time list of mod admins (plaintext Steam ID64s).
    /// Use <see cref="IsAdmin"/> to check membership at runtime.
    /// </summary>
    public static class PlayerLists
    {
        private static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{AlphaPlugin.ModName}.PL");

        private static readonly HashSet<string> _adminIds = new HashSet<string>
        {
            /*Alpha*/ "76561198144499930",
            /*Beta*/  "76561198729588983",
        };

        /// <summary>Returns true if the given Steam ID64 is in the admin list.</summary>
        public static bool IsAdmin(string steamId)
        {
            if (string.IsNullOrEmpty(steamId)) return false;
            return _adminIds.Contains(steamId);
        }
    }
}
