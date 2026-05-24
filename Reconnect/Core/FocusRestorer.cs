using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using PurrNet;
using UnityEngine;

namespace Reconnect.Core
{
    /// <summary>
    /// Attempts to restore focus state after reconnect by programmatically
    /// setting up PlayerFocusController state and calling SetFocus().
    /// </summary>
    public static class FocusRestorer
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Reconnect.FocusRestorer");

        private const float WaitForAreaFixSec = 5f;

        static readonly FieldInfo _simpledFocusTypesField =
            AccessTools.Field(typeof(PlayerFocusController), "_simpledFocusTypes");
        static readonly FieldInfo _currentFocusMainIndexField =
            AccessTools.Field(typeof(PlayerFocusController), "_currentFocusMainIndex");
        static readonly FieldInfo _preFocusPositionField =
            AccessTools.Field(typeof(PlayerFocusController), "_preFocusPosition");
        static readonly FieldInfo _preFocusRotationField =
            AccessTools.Field(typeof(PlayerFocusController), "_preFocusRotation");
        static readonly PropertyInfo _currentFocusAreaControllerProp =
            AccessTools.Property(typeof(PlayerFocusController), "CurrentFocusAreaController");

        /// <summary>
        /// Coroutine that waits for focus area state to be available, then restores focus.
        /// </summary>
        public static IEnumerator TryRestoreFocus(PlayerStateSnapshot state)
        {
            if (!state.WasFocused)
                yield break;

            // Wait for FocusAreaManager.AreaIndexes to be populated (focus area fix runs first)
            float elapsed = 0f;
            while (elapsed < WaitForAreaFixSec)
            {
                FocusAreaManager? fam = NetworkSingleton<FocusAreaManager>.I;
                if (fam != null && fam.AreaIndexes != null)
                    break;
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;
            }

            FocusAreaManager? focusAreaManager = NetworkSingleton<FocusAreaManager>.I;
            if (focusAreaManager == null || focusAreaManager.AreaIndexes == null)
            {
                _log.LogWarning("FocusRestorer: AreaIndexes still null after wait - cannot restore focus.");
                yield break;
            }

            // Small extra delay to let triggers fire and state settle
            yield return new WaitForSeconds(1f);

            PlayerFocusController? focusCtrl = NetworkSingleton<TextChannelManager>.I?.MainFocusController;
            if (focusCtrl == null)
            {
                _log.LogWarning("FocusRestorer: MainFocusController not available.");
                yield break;
            }

            if (focusCtrl.IsFocus)
            {
                _log.LogDebug("FocusRestorer: Player is already in focus - skipping.");
                yield break;
            }

            bool success = RestoreFocusState(focusCtrl, state, focusAreaManager);
            if (success)
            {
                _log.LogInfo($"FocusRestorer: Restored focus ({state.FocusType}) at area {state.FocusAreaId}.");
                Alpha.Core.Util.ChatUtils.AddGlobalNotification("Reconnected - focus restored.");
            }
            else
            {
                _log.LogWarning("FocusRestorer: Could not restore focus automatically.");
                Alpha.Core.Util.ChatUtils.AddGlobalNotification("Reconnected - position restored. Press F to re-enter focus.");
            }
        }

        private static bool RestoreFocusState(
            PlayerFocusController focusCtrl,
            PlayerStateSnapshot state,
            FocusAreaManager focusAreaManager)
        {
            FocusAreaController? areaController = null;

            // Find the area controller if it was an area focus (not free focus)
            if (state.FocusAreaId >= 0)
            {
                List<FocusAreaController> areaControllers = focusAreaManager.AreaControllers;
                if (state.FocusAreaId < areaControllers.Count)
                    areaController = areaControllers[state.FocusAreaId];

                if (areaController == null)
                {
                    _log.LogWarning($"FocusRestorer: Area controller {state.FocusAreaId} not found.");
                    return false;
                }

                // Check if seat is available
                if (!areaController.IsYogaOrIsland && !focusAreaManager.IsAreaEmpty(state.FocusAreaId))
                {
                    _log.LogWarning($"FocusRestorer: Seat {state.FocusAreaId} is occupied.");
                    return false;
                }
            }

            // Set CurrentFocusAreaController via reflection (private setter)
            _currentFocusAreaControllerProp.SetValue(focusCtrl, areaController);

            // Build _simpledFocusTypes from the area (or global settings for free focus)
            List<FocusType> sourceTypes = (areaController == null)
                ? ScriptableSingleton<GameSettings>.I.FocusTypes
                : areaController.FocusAreaSettings.FocusTypes;

            var simpledTypes = new List<FocusType>();
            for (int i = 0; i < sourceTypes.Count; i++)
            {
                if (!simpledTypes.Contains(sourceTypes[i]))
                    simpledTypes.Add(sourceTypes[i]);
            }

            // Find the main index for our saved FocusType
            int mainIndex = simpledTypes.IndexOf(state.FocusType);
            if (mainIndex == -1)
            {
                _log.LogWarning($"FocusRestorer: FocusType {state.FocusType} not available at this area.");
                _currentFocusAreaControllerProp.SetValue(focusCtrl, null);
                return false;
            }

            // Set the private fields
            _simpledFocusTypesField.SetValue(focusCtrl, simpledTypes);
            _currentFocusMainIndexField.SetValue(focusCtrl, mainIndex);

            // Set pre-focus position so "get up" teleports back correctly
            _preFocusPositionField.SetValue(focusCtrl, state.PreFocusPosition);
            _preFocusRotationField.SetValue(focusCtrl, state.PreFocusRotation);

            // Call SetFocus(0) - first sub-activity of the saved type
            focusCtrl.SetFocus(0);

            // Restore _preFocusPosition after SetFocus (it overwrites it with current position)
            _preFocusPositionField.SetValue(focusCtrl, state.PreFocusPosition);
            _preFocusRotationField.SetValue(focusCtrl, state.PreFocusRotation);

            return true;
        }
    }
}
