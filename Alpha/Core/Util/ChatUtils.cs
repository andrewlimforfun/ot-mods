using System;
using System.Text;
using System.Text.RegularExpressions;
using Alpha;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Alpha.Core.Util
{

    public static class ChatUtils
    {
        private static readonly Regex _commonTMPTagRegex = new Regex(
        @"</?(?:color|b|i|u|s|sup|sub|size|alpha|mark|cspace|width|uppercase|lowercase|smallcaps|font|voffset|nobr|noparse|sprite|link|align|rotate|#[0-9a-fA-F]{3,8})[^>]*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public const int MaxMessageLength = 250;

        public static void AddGlobalNotification(string text)
        {
            TextChannelManager textChannelManager = NetworkSingleton<TextChannelManager>.I;
            if (textChannelManager == null)
            {
                return;
            }
            textChannelManager.AddNotification(text);
        }

        public static void SendMessageAsync(string userName, string text, bool IsLocal = false, string chunkStrategy = "word", Vector3? position = null)
        {
            // split text into chunks max 250 chars either using word or default to hard cut strategy
            IStringChunker chunker = chunkStrategy.ToLower() switch
            {
                "word" => new WordBoundaryChunker(),
                _ => new HardCutChunker()
            };

            foreach (var chunk in chunker.Chunk(text, MaxMessageLength))
            {
                SendMessageTruncatedAsync(userName, chunk, IsLocal, position);
            }
        }

        public static void SendMessageTruncatedAsync(string userName, string text, bool IsLocal = false, Vector3? position = null)
        {
            byte[] messageBytes = Encoding.Unicode.GetBytes(text[..Math.Min(MaxMessageLength, text.Length)]);
            byte[] userNameBytes = Encoding.Unicode.GetBytes(userName);

            string steamPlayerId = SteamUtils.GetPlayerSteamID();
            
            TextChannelManager textChannelManager = NetworkSingleton<TextChannelManager>.I;
            Transform mainPlayer = textChannelManager.MainPlayer;
            textChannelManager.SendMessageAsync(messageBytes, userNameBytes, IsLocal, position ?? mainPlayer.position, steamPlayerId);
        }

        public static void CleanCommand()
        {
            // These cleanup patterns follow TextChannelManager.OnEnterPressed logic
            // They will hide the command from the chat instead of sending them to the server

            // Synchronizes the task system with the music state - preventing conflicting gameplay inputs during rhythm/music sections
            LockState lockState = NetworkSingleton<MusicManager>.I.IsActive ? LockState.Music : LockState.Free;
            MonoSingleton<TaskManager>.I.SetLockState(lockState);

            // Removes focus from any currently selected UI element        
            EventSystem.current.SetSelectedGameObject(null);

            // special exception for /help since this command is used by other plugins
            var text = MonoSingleton<UIManager>.I.MessageInput.text;
            // officer balls and jaide compatibility
            if (!text.StartsWith("/help"))
            {
                // Clears the text input field
                MonoSingleton<UIManager>.I.MessageInput.text = "";
            }
        }

        /// <summary>
        /// Removes common TextMeshPro tags from a string to improve readability when relaying messages from external sources that may contain formatting tags.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CleanTMPTags(string input)
        {
            // Remove common TMP tags
            return _commonTMPTagRegex.Replace(input, string.Empty);
        }

        /// <summary>
        /// Injects <paramref name="text"/> into the in-game chat input field and submits
        /// it as if the user had typed it, allowing it to be processed by the game's chat system and other mods' command handlers.
        /// </summary>
        public static void UISendMessage(string text)
        {
            var inputField = MonoSingleton<UIManager>.I.MessageInput;
            inputField.text = text;
            inputField.onSubmit.Invoke(inputField.text);
        }

    }

}
