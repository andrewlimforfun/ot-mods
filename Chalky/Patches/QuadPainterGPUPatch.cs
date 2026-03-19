using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using Chalky.Core.Commands;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.IO;
using BepInEx.Logging;

namespace Chalky.Patches
{
    [HarmonyPatch(typeof(QuadPainterGPU))]

    public class QuadPainterGPUPatch
    {
        static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Chalky.QuadPainterGPUPatch");

        // cached once at startup - avoids repeated reflection lookups per paint call
        private static readonly System.Reflection.FieldInfo _pixelsField =
            typeof(QuadPainterGPU).GetField("_pixelsToUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo _previousUVField =
            typeof(QuadPainterGPU).GetField("previousUV", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.MethodInfo _fillTheBlanksMethod =
            typeof(QuadPainterGPU).GetMethod("FillTheBlanks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        
        // Update prefix - for size > 2 (non-erase), send multiple offset
        // FillTheBlanksRPC calls so ALL clients (modded or not) see the
        // expanded brush via tiled 2×2 blocks.
        
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool UpdatePrefix(QuadPainterGPU __instance)
        {
            if (ChalkyPlugin.EnableFeature == null || ChalkyPlugin.EnableFeature.Value == false)
                return true;

            int size = ChalkyPlugin.chalkySize;
            if (size <= 2)
                return true;

            // replicate the original Update guards
            if (!__instance.IsActive ||
                (!MonoSingleton<DrawingManager>.I.IsHaveChalk() && !MonoSingleton<DrawingManager>.I.IsEraser))
                return false;

            // erasing - let original handle it unchanged
            if (MonoSingleton<DrawingManager>.I.IsEraser)
                return true;

            Vector2 previousUV = (Vector2)_previousUVField.GetValue(__instance);

            if (Input.GetMouseButton(0))
            {
                var ray = MonoSingleton<ClickManager>.I.GetMousePosRay().Item1;
                RaycastHit hit;
                if (!Physics.Raycast(ray, out hit, 10f, 1 << LayerMask.NameToLayer("Drawing")) ||
                    hit.collider.gameObject != __instance.gameObject)
                    return false;

                Vector2 textureCoord = hit.textureCoord;
                Vector2Int gridSize = __instance.gridSize;

                // each default brush stroke covers 2 grid cells, so step by 2
                float stepX = 2f / gridSize.x;
                float stepY = 2f / gridSize.y;
                int tiles = Mathf.CeilToInt(size / 2f);

                int colIndex = MonoSingleton<DrawingManager>.I.ChalkIndex;

                for (int tx = 0; tx < tiles; tx++)
                {
                    for (int ty = 0; ty < tiles; ty++)
                    {
                        Vector2 offsetUV = new Vector2(
                            textureCoord.x + tx * stepX,
                            textureCoord.y + ty * stepY);

                        Vector2 offsetPrevUV = previousUV.x > 0f
                            ? new Vector2(previousUV.x + tx * stepX, previousUV.y + ty * stepY)
                            : previousUV;

                        // broadcast to all other clients (uses existing network protocol)
                        __instance.FillTheBlanksRPC(offsetUV, offsetPrevUV, colIndex, false, false);

                        // paint locally (FillTheBlanks is private => invoke via reflection)
                        _fillTheBlanksMethod.Invoke(__instance,
                            new object[] { offsetUV, offsetPrevUV, colIndex, false, false, true });
                    }
                }

                _previousUVField.SetValue(__instance, textureCoord);
            }

            if (Input.GetMouseButtonUp(0))
            {
                // reset previousUV.x to -1 (same as original)
                Vector2 resetUV = previousUV;
                resetUV.x = -1f;
                _previousUVField.SetValue(__instance, resetUV);
            }

            return false; // skip original Update
        }


    }
}
