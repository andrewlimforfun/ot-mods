using HarmonyLib;
using System.Reflection;

namespace Fomo.Patches
{
    [HarmonyPatch(typeof(GameSettings))]
    public class GameSettingsPatch
    {
        // Called once when the ScriptableSingleton is first accessed
        [HarmonyPatch(nameof(GameSettings.GlobalMessageLimitCount), MethodType.Getter)]
        [HarmonyPostfix]
        public static void GlobalMessageLimitCountPostfix(ref int __result)
        {
            int limit = FomoPlugin.GlobalMessageLimitCount?.Value ?? __result;
            if (limit > 0) __result = limit;
        }

        [HarmonyPatch(nameof(GameSettings.LocalMessageLimitCount), MethodType.Getter)]
        [HarmonyPostfix]
        public static void LocalMessageLimitCountPostfix(ref int __result)
        {
            int limit = FomoPlugin.LocalMessageLimitCount?.Value ?? __result;
            if (limit > 0) __result = limit;
        }
    }
}
