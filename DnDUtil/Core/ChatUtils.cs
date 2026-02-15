using System;
using System.Text;
using DnDUtil;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

public static class ChatUtils
{
    public static void AddGlobalNotification(string text)
    {
        NetworkSingleton<TextChannelManager>.I.AddNotification(text);
    }

    public static string GetUserName()
    {
        return NetworkSingleton<TextChannelManager>.I.UserName;
    }

    public static void SendMessageAsync(string userName, string text, bool Islocal = false)
    {
        TextChannelManager textChannelManager = NetworkSingleton<TextChannelManager>.I;
        Transform mainPlayer = textChannelManager.MainPlayer;
        byte[] messageBytes = Encoding.Unicode.GetBytes(text[..Math.Min(text.Length, 250)]);
        byte[] userNameBytes = Encoding.Unicode.GetBytes(userName);

        // get steam player id via reflection since TextChannelManager doesn't expose it and we need it to send messages
        // alternatively get player id via Steamworks.SteamUser.GetSteamID().ToString()
        var playerId = Traverse.Create(textChannelManager).Field<string>("_playerId").Value ?? "0";

        textChannelManager.SendMessageAsync(messageBytes, userNameBytes, Islocal, mainPlayer.position, playerId);
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
        
        var text = MonoSingleton<UIManager>.I.MessageInput.text;

        // special exception for /help since this command is used by other plugins
        if (!text.StartsWith("/help") || text == "/help dnd")
        {
            // Clears the text input field
            MonoSingleton<UIManager>.I.MessageInput.text = "";
        }
    }

}
