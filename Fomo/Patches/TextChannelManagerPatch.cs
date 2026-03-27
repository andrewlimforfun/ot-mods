using HarmonyLib;
using UnityEngine;
using Fomo.Core;
using System;
using System.Threading.Tasks;
using PurrNet;
using System.Text;
using Alpha.Core.Util;

namespace Fomo.Patches
{
    [HarmonyPatch(typeof(TextChannelManager))]

    public class TextChannelManagerPatch
    {
        const int _selfDistance = 0;
        
        [HarmonyPatch("AddNotification", typeof(string))]
        [HarmonyPostfix]
        public static void AddNotificationPostfix(string text)
        {
            if (FomoPlugin.EnableFeature?.Value == false) return;

            _ = Task.Run(async () =>
            {
                // always clean text because notifications contain usernames
                string cleanText = ChatUtils.CleanTMPTags(text);
                var entry = new ChatEntry(DateTime.Now, cleanText, source: "AN");
                await (FomoPlugin.SinkManager?.BroadcastAsync(entry) ?? Task.CompletedTask);
            });
        }

        [HarmonyPatch("SendMessageAsync", typeof(byte[]), typeof(byte[]), typeof(bool), typeof(Vector3), typeof(string), typeof(RPCInfo))]
        [HarmonyPostfix]
        public static void SendMessageAsyncPostfix(byte[] textBytes, byte[] userName, bool isLocal, Vector3 pos, string playerID, RPCInfo info = default(RPCInfo))
        {
            if (FomoPlugin.EnableFeature?.Value == false) return;

            _ = Task.Run(async () =>
            {
                string channel = isLocal ? "Local" : "Global";
                // always clean username - hard to read otherwise
                string cleanUserName = ChatUtils.CleanTMPTags(Encoding.Unicode.GetString(userName));
                string rawMessage = Encoding.Unicode.GetString(textBytes);
                string cleanMessage = FomoPlugin.CleanChatSinkTags?.Value == true
                    ? ChatUtils.CleanTMPTags(rawMessage)
                    : rawMessage;

                var entry = new ChatEntry(DateTime.Now, cleanMessage, channel: channel, 
                userName: cleanUserName, source: "SM", playerID: playerID, distance: isLocal ? (int) Vector3.Distance(pos, NetworkSingleton<TextChannelManager>.I.MainPlayer.position) : _selfDistance);
                await (FomoPlugin.SinkManager?.BroadcastAsync(entry) ?? Task.CompletedTask);
            });
        }

        [HarmonyPatch("OnChannelMessageReceived", typeof(string), typeof(string), typeof(Vector3), typeof(bool), typeof(int), typeof(string))]
        [HarmonyPostfix]
        public static void OnChannelMessageReceivedPostfix(string userName, string message, Vector3 senderPosition, bool isLocal, int senderIndex, string playerID)
        {
            if (FomoPlugin.EnableFeature?.Value == false) return;

            if (MonoSingleton<DataManager>.I.BanData.IgnorePlayers.Contains(playerID)) return;
            if (MonoSingleton<DataManager>.I.BanData.MutedPlayers.Contains(playerID)) return;

            if (SteamUtils.GetPlayerSteamID() == playerID) return;

            // fire-and-forget task to avoid hitching the main thread with file IO / WebSocket work
            _ = Task.Run(async () =>
            {
                // filter by distance for local messages and include it in the channel label
                string channel = isLocal ? "Local" : "Global";

                int? distance = null;
                if (isLocal)
                {
                    distance = (int) Vector3.Distance(senderPosition, NetworkSingleton<TextChannelManager>.I.MainPlayer.position);
                    int localRange = FomoPlugin.ChatSinkLocalRange?.Value ?? FomoPlugin.DefaultChatSinkLocalRange;
                    if (distance > localRange) return;
                }

                // always clean username - hard to read otherwise
                string cleanUserName = ChatUtils.CleanTMPTags(userName);
                string cleanMessage = FomoPlugin.CleanChatSinkTags?.Value == true
                    ? ChatUtils.CleanTMPTags(message)
                    : message;

                var entry = new ChatEntry(DateTime.Now, cleanMessage, channel: channel, userName: cleanUserName, 
                    source: "OC", playerID: playerID, distance: distance);
                await (FomoPlugin.SinkManager?.BroadcastAsync(entry) ?? Task.CompletedTask);
            });
        }

    }
}
