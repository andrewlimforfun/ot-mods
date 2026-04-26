using System.Text;
using BepInEx.Logging;
using HarmonyLib;

namespace Fomo.Patches
{
    [HarmonyPatch(typeof(PlayerCustomizationController))]
    public static class PlayerCustomizationControllerPatch
    {
        private static readonly ManualLogSource _log =
            Logger.CreateLogSource($"{FomoPlugin.ModName}.PCCP");

        // Runs server-side when a player joins. Adds a host-only notification showing
        // the joining player's display name and Steam ID.
        [HarmonyPostfix]
        [HarmonyPatch("UpdateOnNewPeopleJoin_Original_0")]
        public static void UpdateOnNewPeopleJoin_Original_0_Postfix(string playerSteamID, PlayerIDInfo playerIDInfo)
        {
            string playerName = Encoding.Unicode.GetString(playerIDInfo.Name);
            NetworkSingleton<TextChannelManager>.I.AddNotification($"{playerName} - {playerSteamID}");
        }
    }
}
