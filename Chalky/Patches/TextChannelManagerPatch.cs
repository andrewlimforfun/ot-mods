using HarmonyLib;
using UnityEngine;
using System;
using BepInEx.Logging;
using Alpha.Core.Util;

namespace Chalky.Patches
{
    [HarmonyPatch(typeof(TextChannelManager))]

    public class TextChannelManagerPatch
    {
        static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Chalky.TextChannelManagerPatch");
    }
}
