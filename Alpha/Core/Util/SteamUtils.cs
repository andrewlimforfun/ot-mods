using Steamworks;

namespace Alpha.Core.Util
{

    public static class SteamUtils
    {
        public static bool IsSteamID(string value) =>
            value != null && value.Length == 17 && ulong.TryParse(value, out _);

        public static string GetPlayerSteamID()
        {
            return Steamworks.SteamUser.GetSteamID().ToString();
        }

        public static string GetSteamPersonaName()
        {
            return Steamworks.SteamFriends.GetPersonaName();
        }

        public static string GetSteamPersonaName(string steamId)
        {
            return Steamworks.SteamFriends.GetFriendPersonaName(new CSteamID(ulong.Parse(steamId)));
        }

        public static string GetLobbyOwnerSteamID()
        {
            string lobbyCode = MonoSingleton<MultiplayerManager>.I.LobbyCode;
            var lobbyId = new CSteamID(ulong.Parse(lobbyCode));
            string hostSteamId = Steamworks.SteamMatchmaking.GetLobbyOwner(lobbyId).ToString();
            return hostSteamId;
        }
    }
}
