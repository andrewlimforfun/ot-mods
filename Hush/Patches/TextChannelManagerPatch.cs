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


            // Read the arguments from the packet payload
            var reader = BitPackerPool.Get(packet.data);

            byte[] textBytes = null!;
            Packer<byte[]>.Read(reader, ref textBytes);
            byte[] userNameBytes = null!;
            Packer<byte[]>.Read(reader, ref userNameBytes);
            bool isLocal = default;
            Packer<bool>.Read(reader, ref isLocal);
            UnityEngine.Vector3 pos = default;
            Packer<UnityEngine.Vector3>.Read(reader, ref pos);
            string playerID = null!;
            Packer<string>.Read(reader, ref playerID);

            reader.Dispose();

            string userName = Encoding.Unicode.GetString(userNameBytes);
            string userNameClean = ChatUtils.CleanTMPTags(userName);            

            if (HushSettings.VerboseLogging) _log.LogDebug($"[Server] HandleRPCGenerated_0: intercept {userNameClean} ({playerID})");            

            // Drop messages from muted players before any further processing
            if (HushPlugin.MuteManager == null)
                _log.LogWarning("[Server] MuteManager is null - mute check skipped");
            else if (HushPlugin.MuteManager.IsMuted(playerID))
            {
                ChatUtils.AddGlobalNotification($"Muted message from {userName} ({playerID}).");
                _log.LogInfo($"[Server] Blocked message from muted player {userNameClean} ({playerID}).");
                return false;
            }
            else if (HushSettings.VerboseLogging)
                _log.LogDebug($"[Server] {userNameClean} ({playerID}) is not muted, proceeding");

            // Decode the chat text
            string text = Encoding.Unicode.GetString(textBytes);

            // Relay sentinel: whitelisted delegates can request a timed mute via a chat message.
            // Always suppressed - never relayed to clients regardless of whitelist outcome.
            const string RelayPrefix = "hush:";
            if (text.Trim().StartsWith(RelayPrefix, StringComparison.Ordinal))
            {
                // check if delegate
                if (HushPlugin.MuteManager?.IsDelegate(playerID) == true)
                {
                    _log.LogInfo($"[Relay] Relay accepted from delegate {userNameClean} ({playerID}): {text}");
                    try { ExecuteRelay(text, playerID); }
                    catch (Exception ex) { _log.LogError($"[Relay] Unhandled exception executing relay from {userNameClean} ({playerID}): {ex}"); }
                }
                // admin override
                else if (PlayerLists.IsAdmin(playerID))
                {
                    _log.LogInfo($"[Relay] Relay accepted from admin {userNameClean}: {text}");
                    try { ExecuteRelay(text, playerID); }
                    catch (Exception ex) { _log.LogError($"[Relay] Unhandled exception executing relay from admin {userNameClean} ({playerID}): {ex}"); }
                }
                else
                    _log.LogWarning($"[Relay] Relay rejected from non-delegate {userNameClean} ({playerID}): {text}");
                return false;
            }

            // Apply word/pattern filter (only when filter is active)
            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null || !filter.Enabled)
                return true;

            FilterResult result = filter.Apply(text);

            if (result.WasBlocked)
            {
                ChatUtils.AddGlobalNotification($"Filter blocked message from {userName} ({playerID}).");
                _log.LogInfo($"[Server] Blocked message from {userNameClean} ({playerID}): \"{text}\"");
                return false; // Skip entirely - message is never relayed
            }

            if (!result.WasModified)
                return true; // Nothing to change

            _log.LogInfo($"[Server] Censored message from {userNameClean} ({playerID}).");

            // Rebuild the payload with censored text
            byte[] censoredBytes = Encoding.Unicode.GetBytes(result.Text);

            var writer = BitPackerPool.Get();
            Packer<byte[]>.Write(writer, censoredBytes);
            Packer<byte[]>.Write(writer, userNameBytes);
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
        /// Wires up a <see cref="RelayExecutor"/> with live game-API implementations and runs it.
        /// </summary>
        private static void ExecuteRelay(string payload, string senderSteamId)
        {
            PlayerMuteManager? mutes = HushPlugin.MuteManager;
            if (mutes == null) return;

            var executor = new RelayExecutor(
                mutes: mutes,
                ban: (id, name) => HushPlugin.BanManager?.Ban(id, name) ?? false,
                unmute: id => mutes.Unmute(id),
                resolveName: id => PlayerUtils.FindPlayerBySteamID(id)?.UserNameClean,
                resolveQuery: q => PlayerUtils.FindPlayerByQuery(q)?.SteamID,
                notify: ChatUtils.AddGlobalNotification,
                saveMutes: HushPlugin.SaveMutes,
                log: _log
            );
            executor.Execute(payload, senderSteamId);
        }

#pragma warning disable Harmony003
        private static string FormatDuration(TimeSpan ts) => DurationFormatter.Format(ts);
#pragma warning restore Harmony003

        // -- Client-side patches ------------------------------------------------------

        /// <summary>
        /// Intercepts all incoming messages from others before display.
        /// Backup relay path: if a <c>hush:</c> command somehow escaped
        /// <see cref="HandleRPCGenerated_0_Prefix"/>, re-execute the relay on the
        /// server-host and always suppress the raw command from appearing in chat.
        /// Also suppresses the notification badge for filter-blocked messages.
        /// </summary>
        [HarmonyPatch("OnChannelMessageReceived")]
        [HarmonyPrefix]
        public static bool OnChannelMessageReceived_Prefix(string message, string playerID)
        {
            if (string.IsNullOrEmpty(message))
                return true;

            // Backup relay: hush: commands should never reach this point.
            // If they do, the message was already relayed to all clients - let it show in chat
            // as visible evidence of the failure. On the server-host, still execute the relay
            // so mute/ban logic takes effect anyway.
            const string RelayPrefix = "hush:";
            if (message.Trim().StartsWith(RelayPrefix, StringComparison.Ordinal))
            {
                if (NetworkSingleton<TextChannelManager>.I?.isServer == true)
                {
                    _log.LogWarning($"[Relay] Backup path triggered - relay escaped RPC intercept. sender=({playerID}): {message}");
                    try { ExecuteRelay(message, playerID); }
                    catch (Exception ex) { _log.LogError($"[Relay] Backup path unhandled exception from ({playerID}): {ex}"); }
                }
                // Fall through - message remains visible in chat on all clients as failure evidence.
            }

            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null || !filter.Enabled)
                return true;
            
            FilterResult result = filter.Apply(message);
            if (result.WasBlocked) _log.LogInfo("Suppressed notification for blocked message: " + message);
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

            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null || !filter.Enabled)
                return true;

            FilterResult result = filter.Apply(text);
            if (result.WasBlocked)
            {
                _log.LogInfo("Blocked message in UI: " + result.Text);
                return false;
            }
            if (result.WasModified)
            {
                _log.LogInfo("Censored message in UI: " + result.Text);
                text = result.Text;
            }
            return true;
        }
    }
}
