using Alpha.Core.Command;
using BepInEx.Logging;
using UnityEngine;

namespace Alpha.Core.Commands
{
    /// <summary>
    /// /alphaquit - immediately quits the game.
    /// </summary>
    public class AlphaQuitCommand : IChatCommand
    {
        static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Alpha.AQC");
        public string Name => "alphaquit";
        public string ShortName => "aq";
        public string Description => "Quit the game. Usage: /alphaquit";
        public string Namespace => "alpha";
        // not for casual user
        public bool IsHidden => true;

        public void Execute(string[] args)
        {
            _log.LogWarning("Quitting game via /alphaquit command.");
            Application.Quit();
        }
    }
}
