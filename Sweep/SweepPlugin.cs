using System;
using System.Collections;
using System.Xml;
using Alpha;
using Alpha.Core.Util;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Sweep.Core.Commands;
using UnityEngine;

namespace Sweep
{
    [BepInPlugin(SweepPlugin.ModGUID, SweepPlugin.ModName, SweepPlugin.ModVersion)]
    public class SweepPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.andrewlin.ontogether.sweep";
        public const string ModName = "Sweep";
        public const string ModVersion = BuildInfo.Version;

        internal static ManualLogSource Log = null!;

        public static ConfigEntry<string>? Interval { get; private set; }

        private Coroutine? _sweepCoroutine;

        void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");

            InitConfig();

            AlphaPlugin.CommandManager?.Register(new SweepIntervalCommand());
            AlphaPlugin.CommandManager?.Register(new SweepNowCommand());

            RestartCoroutine();
        }

        void InitConfig()
        {
            Interval = Config.Bind("General", "Interval", "PT30M",
                "How often to unload unused assets. ISO 8601 duration. Examples: PT30M, PT1H, PT1H30M");
        }

        internal static TimeSpan GetInterval()
        {
            if (Interval == null) return TimeSpan.FromMinutes(30);
            if (TimeUtils.TryParseDuration(Interval.Value, out TimeSpan result) && result.TotalSeconds > 0)
                return result;
            return TimeSpan.FromMinutes(30);
        }

        internal static string FormatInterval()
        {
            TimeSpan ts = GetInterval();
            return XmlConvert.ToString(ts);
        }

        internal void RestartCoroutine()
        {
            if (_sweepCoroutine != null)
                StopCoroutine(_sweepCoroutine);
            _sweepCoroutine = StartCoroutine(SweepLoop());
        }

        private IEnumerator SweepLoop()
        {
            while (true)
            {
                float seconds = (float)GetInterval().TotalSeconds;
                yield return new WaitForSecondsRealtime(seconds);
                Log.LogInfo("Sweep: Unload unused assets...");
                Resources.UnloadUnusedAssets();
            }
        }

        private static SweepPlugin? _instance;

        void OnEnable() => _instance = this;

        internal static SweepPlugin? Instance => _instance;

        void OnDestroy()
        {
            _instance = null;
        }
    }
}
