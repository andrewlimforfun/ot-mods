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
    /// text before the relay, every client receives the filtered version — host-only install required.
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
            if (!asServer || HushPlugin.FilterManager == null || !HushPlugin.FilterManager.Enabled)
                return true;

            ChatFilterManager? filter = HushPlugin.FilterManager;
            if (filter == null || !filter.Enabled)
                return true;

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

            // Drop messages from muted players before any further processing
            if (HushPlugin.MuteManager?.IsMuted(playerID) == true)
            {
                ChatUtils.AddGlobalNotification($"Muted message from {playerID}.");
                _log.LogInfo($"[Server] Blocked message from muted player {playerID}.");
                return false;
            }

            // Decode the chat text and apply filter
            string text = Encoding.Unicode.GetString(textBytes);
            FilterResult result = filter.Apply(text);

            if (result.WasBlocked)
            {
                ChatUtils.AddGlobalNotification($"Filter blocked message from {playerID}.");
                _log.LogInfo($"[Server] Blocked message from {playerID}: \"{text}\"");
                return false; // Skip entirely — message is never relayed
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

        /// <summary>
        /// Client-side: intercepts all incoming messages from others before display.
        /// Suppresses the notification badge for blocked messages entirely.
        /// </summary>
        [HarmonyPatch("OnChannelMessageReceived")]
        [HarmonyPrefix]
        public static bool OnChannelMessageReceived_Prefix(string message)
        {
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
