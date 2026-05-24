using System.Collections;
using BepInEx.Logging;
using PurrNet;
using UnityEngine;

namespace Reconnect.Core
{
    /// <summary>
    /// After spawning, if FocusAreaManager.AreaIndexes is still null
    /// (Init RPC missed due to bufferLast:false race), re-requests the state
    /// from the server by calling UpdateFocusAreasOnNewPeople again.
    /// </summary>
    public static class FocusAreaFix
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Reconnect.FocusAreaFix");

        private const float RetryDelaySec = 5f;

        /// <summary>
        /// Starts a coroutine that waits then re-requests focus area state if needed.
        /// </summary>
        public static IEnumerator TryRepairAfterSpawn()
        {
            yield return new WaitForSeconds(RetryDelaySec);

            if (!ShouldRepair())
            {
                _log.LogDebug("FocusAreaFix: AreaIndexes already populated - no repair needed.");
                yield break;
            }

            _log.LogWarning("FocusAreaFix: AreaIndexes is null after spawn - requesting re-sync from server.");
            Apply();
        }

        /// <summary>
        /// Immediately attempts to repair focus area state by re-sending the
        /// UpdateFocusAreasOnNewPeople ServerRpc.
        /// </summary>
        public static bool Apply()
        {
            FocusAreaManager? fam = NetworkSingleton<FocusAreaManager>.I;
            if (fam == null)
            {
                _log.LogWarning("FocusAreaFix: FocusAreaManager singleton not available.");
                return false;
            }

            NetworkTransform? netTransform = NetworkSingleton<TextChannelManager>.I?.MainNetTransform;
            if (netTransform == null)
            {
                _log.LogWarning("FocusAreaFix: MainNetTransform not available - cannot request re-sync.");
                return false;
            }

            fam.UpdateFocusAreasOnNewPeople(netTransform);
            _log.LogInfo("FocusAreaFix: Sent UpdateFocusAreasOnNewPeople ServerRpc to re-sync focus areas.");
            return true;
        }

        /// <summary>
        /// Returns true if focus area state is broken and needs repair.
        /// </summary>
        public static bool ShouldRepair()
        {
            FocusAreaManager? fam = NetworkSingleton<FocusAreaManager>.I;
            return fam != null && fam.AreaIndexes == null;
        }
    }
}
