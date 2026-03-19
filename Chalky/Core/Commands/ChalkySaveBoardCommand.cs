using Chalky.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Chalky.Core.Commands
{
    /// <summary>
    /// /chalkysaveboard [name] [index]
    ///
    /// Saves the specified (or currently active) chalkboard to disk.
    /// Anyone - host or client - can save; it is a local read of PaintColors.
    ///
    /// Args:
    ///   name  - file name without extension (default: "default")
    ///   index - board index 0/1/2 (default: currently active board)
    /// </summary>
    public class ChalkySaveBoardCommand : IChatCommand
    {
        public const string CMD = "chalkysaveboard";
        public string Name => CMD;
        public string ShortName => "csb";
        public string Description =>
            "Save a chalkboard to disk. Usage: /chalkysaveboard [name] [index]. " +
            "Omit [index] to use the currently active board.";

        public string Namespace => "chalky";
        public void Execute(string[] args)
        {
            string name = args.Length >= 1 ? args[0] : "default";
            string? indexArg = args.Length >= 2 ? args[1] : null;

            int index = BoardIO.ResolveBoard(indexArg);
            if (index < 0)
            {
                ChatUtils.AddGlobalNotification(
                    "Could not determine board index. " +
                    "Interact with a board first, or pass an explicit index. e.g. /chalkysaveboard rat_king 1");
                return;
            }

            BoardIO.SaveBoard(index, name);
            ChatUtils.AddGlobalNotification(
                $"Board {index} saved as '{name}' in {ChalkyPlugin.SaveDir}");
        }
    }
}
