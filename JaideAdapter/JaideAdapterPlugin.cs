using System;
using System.Collections;
using Alpha;
using Alpha.Core.Command;
using BepInEx;
using BepInEx.Logging;
using CommandAPI;
using UnityEngine;

namespace JaideAdapter
{
    [BepInPlugin(JaideAdapterPlugin.ModGUID, JaideAdapterPlugin.ModName, JaideAdapterPlugin.ModVersion)]
    [BepInDependency(AlphaPlugin.ModGUID)]
    [BepInDependency("com.on-together-mods.commandapi", BepInDependency.DependencyFlags.SoftDependency)]
    public class JaideAdapterPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.andrewlin.ontogether.jaideadapter";
        public const string ModName = "JaideAdapter";
        public const string ModVersion = BuildInfo.Version;

        internal static ManualLogSource Log = null!;

        void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"{ModName} v{ModVersion} is loaded!");
        }

        IEnumerator Start()
        {
            // Yield one frame so all other plugins' Awake() have finished registering commands.
            yield return null;
            SyncToCommandAPI();
        }

        static void SyncToCommandAPI()
        {
            if (AlphaPlugin.CommandManager == null)
            {
                Log.LogWarning("Alpha CommandManager is null - skipping CommandAPI sync.");
                return;
            }

            int count = 0;
            foreach (IChatCommand cmd in AlphaPlugin.CommandManager.GetAllCommands())
            {
                if (cmd.IsHidden) continue;

                string category = string.IsNullOrWhiteSpace(cmd.Namespace) ? "General" : cmd.Namespace;
                CommandRegistry.Register(
                    name: cmd.Name,
                    category: category,
                    handler: _ => { },
                    description: cmd.Description,
                    parameters: Array.Empty<Parameter>()
                );
                count++;
            }

            Log.LogInfo($"Synced {count} Alpha commands to Jaide CommandAPI.");
        }
    }
}
