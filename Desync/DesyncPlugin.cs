using System;
using System.Collections;
using System.Collections.Concurrent;
using Alpha;
using Alpha.Core.Util;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Desync.Core;
using Desync.Core.Commands;
using Desync.Core.Fixes;
using HarmonyLib;
using PurrNet;
using UnityEngine;

namespace Desync
{
    [BepInPlugin(DesyncPlugin.ModGUID, DesyncPlugin.ModName, DesyncPlugin.ModVersion)]
    public class DesyncPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.andrewlin.ontogether.desync";
        public const string ModName = "Desync";
        public const string ModVersion = BuildInfo.Version;

        internal static ManualLogSource Log = null!;

        public static ConfigEntry<bool>? Enabled { get; private set; }
        public static ConfigEntry<float>? MonitorIntervalSec { get; private set; }
        public static ConfigEntry<bool>? FixLobbyRefresh { get; private set; }
        public static ConfigEntry<bool>? FixHostIdentity { get; private set; }
        public static ConfigEntry<bool>? FixPlayerListRepair { get; private set; }
        public static ConfigEntry<bool>? FixPersonaCache { get; private set; }

        internal static LobbyHealthMonitor? Monitor { get; private set; }

        private Coroutine? _monitorCoroutine;

        void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");
            InitConfig();

            AlphaPlugin.CommandManager?.Register(new DesyncToggleCommand());
            AlphaPlugin.CommandManager?.Register(new DesyncStatusCommand());
            AlphaPlugin.CommandManager?.Register(new DesyncFixCommand());
            AlphaPlugin.CommandManager?.Register(new DesyncIntervalCommand());
        }

        void InitConfig()
        {
            Enabled = Config.Bind("General", "Enabled", true,
                "Enable or disable Desync monitoring and fixes.");

            MonitorIntervalSec = Config.Bind("Monitor", "IntervalSec", 600f,
                "Seconds between health checks (minimum 60).");

            FixLobbyRefresh = Config.Bind("Fix", "LobbyRefresh", true,
                "H1: Host periodically refreshes lobby metadata to prevent Steam timeout.");
            FixHostIdentity = Config.Bind("Fix", "HostIdentity", true,
                "H2: Detect when local player is lobby owner but NetworkManager.isHost is false.");
            FixPlayerListRepair = Config.Bind("Fix", "PlayerListRepair", true,
                "H3: Detect and log when host is missing from player list.");
            FixPersonaCache = Config.Bind("Fix", "PersonaCache", true,
                "H4: Periodically refresh Steam persona cache for lobby players.");
        }

        void Start()
        {
            _monitorCoroutine = StartCoroutine(MonitorLoop());
        }

        IEnumerator MonitorLoop()
        {
            // Wait for game to fully initialize
            yield return new WaitForSeconds(10f);

            while (true)
            {
                float interval = Mathf.Max(60f, MonitorIntervalSec?.Value ?? 300f);
                yield return new WaitForSeconds(interval);

                if (Enabled?.Value != true) continue;

                try
                {
                    RunHealthCheck();
                }
                catch (Exception ex)
                {
                    Log.LogError($"Health check failed: {ex.Message}");
                }
            }
        }

        void RunHealthCheck()
        {
            DesyncDiagnostics diag = DesyncDiagnostics.Capture();
            Monitor = new LobbyHealthMonitor(diag);

            if (Monitor.IsHealthy)
            {
                Log.LogDebug("Health check: all OK.");
                return;
            }

            Log.LogWarning($"Health check detected issues: {Monitor.Summary}");

            // Apply fixes
            if (FixLobbyRefresh?.Value == true && Monitor.HasLobbyMetadataIssue)
                LobbyRefreshFix.Apply(diag);

            if (FixHostIdentity?.Value == true && Monitor.HasHostIdentityIssue)
                HostIdentityFix.Apply(diag);

            if (FixPlayerListRepair?.Value == true && Monitor.HasPlayerListIssue)
                PlayerListRepairFix.Apply(diag);

            if (FixPersonaCache?.Value == true && Monitor.HasPersonaCacheIssue)
                PersonaCacheFix.Apply(diag);
        }

        void OnDestroy()
        {
            if (_monitorCoroutine != null)
                StopCoroutine(_monitorCoroutine);
        }
    }
}
