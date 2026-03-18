using System;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using Alpha;
using HarmonyLib;
using PurrNet;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Alpha.Core.Util
{
    public static class PlayerUtils
    {
        private static ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{AlphaPlugin.ModName}.PU");
        private static string _steamPlayerIdString = string.Empty;
        public static string GetUserName()
        {
            return NetworkSingleton<TextChannelManager>.I.UserName;
        }

        public static string GetUserNameNoFormat()
        {
            return ChatUtils.CleanTMPTags(GetUserName());
        }

        public static string GetSteamPlayerIdString()
        {
            if (!string.IsNullOrEmpty(_steamPlayerIdString))
            {
                return _steamPlayerIdString;
            }

            _steamPlayerIdString = Traverse.Create(NetworkSingleton<TextChannelManager>.I).Field<string>("_playerId").Value ?? string.Empty;
            return _steamPlayerIdString;
        }

        public static PlayerID? GetPlayerId()
        {
            try
            {
                var controller = NetworkSingleton<TextChannelManager>.I;
                PlayerID? playerId = controller?.localPlayer;
                return playerId;

            }
            catch (Exception ex)
            {
                _log.LogError($"Error getting player ID: {ex}");
                return null;
            }
        }

        public static PlayerID? GetPlayerIdForced()
        {
            try
            {
                var controller = NetworkSingleton<TextChannelManager>.I;
                PlayerID? playerId = controller?.localPlayerForced;
                return playerId;

            }
            catch (Exception ex)
            {
                _log.LogError($"Error getting player ID: {ex}");
                return null;
            }
        }
    }
}
