using System;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using PurrNet;
using PurrNet.Packing;
using PurrNet.Transports;
using Hush.Core;
using Alpha.Core.Util;

namespace Hush.Patches
{
    /// <summary>
    /// Patches the server-side RPC handler for chat messages.
    /// When <c>asServer == true</c>, this prefix runs before <c>ValidateReceivingRPC</c>
    /// which relays the packet to all observers. By rebuilding <c>packet.data</c> with censored
    /// text before the relay, every client receives the filtered version - host-only install required.
    /// </summary>
    [HarmonyPatch(typeof(TextChannelManager))]
    public static class TextChannelManagerPatch
    {
        private static readonly ManualLogSource _log =
            Logger.CreateLogSource($"{HushPlugin.ModName}.TCMP");

        /// <summary>
        /// Intercepts the chat RPC on the server before the relay broadcast.
        /// Returns false to skip the original method entirely (blocks the message).
        /// </summary>
        [HarmonyPatch("HandleRPCGenerated_0")]
        [HarmonyPrefix]
        public static bool HandleRPCGenerated_0_Prefix(BitPacker stream, ref RPCPacket packet, RPCInfo info, bool asServer)
        {
            // Only intercept on the server relay path
            if (!asServer)
                return true;

            _log.LogDebug($"[Server] HandleRPCGenerated_0: intercepted packet (asServer={asServer})");

            // Read the arguments from the packet payload
            var reader = BitPackerPool.Get(packet.data);

            byte[] textBytes = null!;
            Packer<byte[]>.Read(reader, ref textBytes);
            byte[] userName = null!;
            Packer<byte[]>.Read(reader, ref userName);
            bool isLocal = default;
            Packer<bool>.Read(reader, ref isLocal);
            UnityEngine.Vector3 pos = default;
            Packer<UnityEngine.Vector3>.Read(reader, ref pos);
            string playerID = null!;
            Packer<string>.Read(reader, ref playerID);

            reader.Dispose();

            _log.LogDebug($"[Server] Message from playerID={playerID}, isLocal={isLocal}");

            // Drop messages from muted players before any further processing
            if (HushPlugin.MuteManager == null)
                _log.LogWarning("[Server] MuteManager is null - mute check skipped");
            else if (HushPlugin.MuteManager.IsMuted(playerID))
            {
                PlayerDetail? playerDetail = PlayerUtils.FindPlayerBySteamID(playerID);
                ChatUtils.AddGlobalNotification($"Muted message from {playerDetail?.UserName ?? "Unknown"} ({playerID}).");
                _log.LogInfo($"[Server] Blocked message from muted player {playerDetail?.UserNameClean ?? "Unknown"} ({playerID}).");
                return false;
            }
            else
                _log.LogDebug($"[Server] {playerID} is not muted, proceeding");

            // Decode the chat text
            string text = Encoding.Unicode.GetString(textBytes);

            // Relay sentinel: whitelisted delegates can request a timed mute via a chat message.
            // Always suppressed - never relayed to clients regardless of whitelist outcome.
            const string RelayPrefix = "hush:";
            if (text.StartsWith(RelayPrefix, StringComparison.Ordinal))
            {
                if (HushPlugin.MuteManager?.IsDelegate(playerID) == true)
                {
                    _log.LogInfo($"[Server] Relay accepted from delegate {playerID}.");
                    ExecuteRelay(text.Substring(RelayPrefix.Length), playerID);
                }
                else
                    _log.LogWarning($"[Server] Relay rejected from non-delegate {playerID}.");
                return false;
            }

            // Apply word/pattern filter (only when filter is active)
            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null || !filter.Enabled)
                return true;

            FilterResult result = filter.Apply(text);

            if (result.WasBlocked)
            {
                ChatUtils.AddGlobalNotification($"Filter blocked message from {playerID}.");
                _log.LogInfo($"[Server] Blocked message from {playerID}: \"{text}\"");
                return false; // Skip entirely - message is never relayed
            }

            if (!result.WasModified)
                return true; // Nothing to change

            _log.LogInfo($"[Server] Censored message from {playerID}.");

            // Rebuild the payload with censored text
            byte[] censoredBytes = Encoding.Unicode.GetBytes(result.Text);

            var writer = BitPackerPool.Get();
            Packer<byte[]>.Write(writer, censoredBytes);
            Packer<byte[]>.Write(writer, userName);
            Packer<bool>.Write(writer, isLocal);
            Packer<UnityEngine.Vector3>.Write(writer, pos);
            Packer<string>.Write(writer, playerID);

            int byteLen = writer.positionInBytes;
            byte[] newBuf = new byte[byteLen];
            System.Array.Copy(writer.buffer, 0, newBuf, 0, byteLen);
            writer.Dispose();

            packet.data = new ByteData(newBuf, 0, byteLen);

            return true;
        }

        // -- Relay helpers --------------------------------------------------------------

        /// <summary>
        /// Parses and executes a relay payload sent by a whitelisted delegate.
        /// Payload format: <c>tmute:&lt;targetSteamId&gt;:&lt;seconds&gt;</c>
        /// </summary>
        private static void ExecuteRelay(string payload, string senderSteamId)
        {
            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            int cmdEnd = payload.IndexOf(':');
            if (cmdEnd < 0) { _log.LogWarning($"[Relay] Malformed payload from {senderSteamId}: {payload}"); return; }

            string cmd = payload.Substring(0, cmdEnd);
            string rest = payload.Substring(cmdEnd + 1);

            string senderName = PlayerUtils.FindPlayerBySteamID(senderSteamId)?.UserNameClean ?? senderSteamId;

            switch (cmd)
            {
                case "tmute":
                {
                    int lastColon = rest.LastIndexOf(':');
                    if (lastColon < 0) { _log.LogWarning($"[Relay] Bad tmute args from {senderSteamId}: {rest}"); return; }
                    string targetId = rest.Substring(0, lastColon);
                    if (!int.TryParse(rest.Substring(lastColon + 1), out int secs) || secs <= 0)
                    { _log.LogWarning($"[Relay] Bad duration from {senderSteamId}: {rest}"); return; }

                    string displayName = PlayerUtils.FindPlayerBySteamID(targetId)?.UserNameClean ?? targetId;
                    mutes.MuteFor(targetId, TimeSpan.FromSeconds(secs));
                    HushPlugin.SaveMutes();
                    string dur = FormatDuration(TimeSpan.FromSeconds(secs));
                    ChatUtils.AddGlobalNotification($"Hush: {senderName} muted {displayName} for {dur} (delegated).");
                    _log.LogInfo($"[Relay] {senderSteamId} muted {targetId} for {secs}s.");
                    break;
                }
                case "ban":
                {
                    string targetId = rest;
                    string displayName = PlayerUtils.FindPlayerBySteamID(targetId)?.UserNameClean ?? targetId;
                    if (HushPlugin.BanManager?.Ban(targetId, displayName) == true)
                    {
                        ChatUtils.AddGlobalNotification($"Hush: {senderName} banned {displayName} (delegated).");
                        _log.LogInfo($"[Relay] {senderSteamId} banned {targetId}.");
                    }
                    break;
                }
                default:
                    _log.LogWarning($"[Relay] Unknown command '{cmd}' from {senderSteamId}.");
                    break;
            }
        }

        // Harmony003 is a false positive here: the analyzer treats every method inside a
        // [HarmonyPatch] class as a patch method and raises false "parameter modified" warnings
        // for value-type property reads.
#pragma warning disable Harmony003
        private static string FormatDuration(TimeSpan ts)
        {
            if (ts.TotalSeconds < 60) return $"{(int)ts.TotalSeconds}s";
            if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes}m";
            if (ts.TotalHours < 24)
            {
                string h = $"{ts.Hours}h";
                return ts.Minutes > 0 ? $"{h} {ts.Minutes}m" : h;
            }
            return $"{(int)ts.TotalDays}d";
        }
#pragma warning restore Harmony003

        // -- Client-side patches ------------------------------------------------------

        /// <summary>
        /// Client-side: intercepts all incoming messages from others before display.
        /// Suppresses the notification badge for blocked messages entirely.
        /// </summary>
        [HarmonyPatch("OnChannelMessageReceived")]
        [HarmonyPrefix]
        public static bool OnChannelMessageReceived_Prefix(string message)
        {
            if (string.IsNullOrEmpty(message))
                return true;
                
            // Suppress relay sentinels that loop back to the host's local client
            if (message.StartsWith("hush:", StringComparison.Ordinal))
            {
                _log.LogDebug("[Client] Suppressed relay sentinel in notification.");
                return false;
            }

            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null || !filter.Enabled)
                return true;
            
            FilterResult result = filter.Apply(message);
            if (result.WasBlocked) _log.LogDebug("[Client] Suppressed notification for blocked message.");
            return !result.WasBlocked;
        }

        /// <summary>
        /// Client-side: intercepts all messages at the final UI display step (own + others').
        /// Applies censoring in-place, or suppresses the message if blocked.
        /// </summary>
        [HarmonyPatch("AddMessageUI")]
        [HarmonyPrefix]
        public static bool AddMessageUI_Prefix(ref string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            // Suppress relay sentinels that loop back to the host's local client
            if (text.StartsWith("hush:", StringComparison.Ordinal))
            {
                _log.LogDebug("[Client] Suppressed relay sentinel in UI.");
                return false;
            }

            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null || !filter.Enabled)
                return true;

            FilterResult result = filter.Apply(text);
            if (result.WasBlocked)
            {
                _log.LogDebug("[Client] Blocked message in UI.");
                return false;
            }
            if (result.WasModified)
            {
                _log.LogDebug("[Client] Censored message in UI.");
                text = result.Text;
            }
            return true;
        }
    }
}
