using HarmonyLib;
using UnityEngine;
using Alpha.Core;
using System;
using System.Threading.Tasks;
using PurrNet;
using System.Text;
using Alpha.Core.Util;

namespace Alpha.Patches
{
    [HarmonyPatch(typeof(TextChannelManager))]

    public class TextChannelManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix()
        {
            AlphaPlugin.ServerJoinTime = DateTime.UtcNow;
        }

        [HarmonyPatch("OnEnterPressed")]
        [HarmonyPrefix]
        public static void OnEnterPressedPrefix()
        {
            string text = MonoSingleton<UIManager>.I.MessageInput.text;

            if (string.IsNullOrEmpty(text) || !text.StartsWith('/') || AlphaPlugin.CommandManager == null)
                return;

            bool isProcessed = AlphaPlugin.CommandManager.ProcessInput(text);

            if (AlphaPlugin.ShowCommand?.Value == false && isProcessed)
                ChatUtils.CleanCommand();
        }
    }
}
