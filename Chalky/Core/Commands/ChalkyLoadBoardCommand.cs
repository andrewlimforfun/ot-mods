using Chalky.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Chalky.Core.Commands
{
    /// <summary>
    /// /chalkyloadboard [name] [index]
    ///
    /// Loads a saved chalkboard from disk, rebuilds the render texture locally,
    /// and broadcasts the state to all currently connected players via GetQuadImage.
    /// Late-joiners receive it automatically through the existing ReadPixelData RPC flow.
    ///
    /// Requires you to be the host (board.isServer == true).
    ///
    /// Args:
    ///   name  - file name without extension (default: "default")
    ///   index - board index 0/1/2 (default: currently active board)
    /// </summary>
    public class ChalkyLoadBoardCommand : IChatCommand
    {
        public const string CMD = "chalkyloadboard";
        public string Name => CMD;
        public string ShortName => "clb";
        public string Description =>
            "Load a chalkboard from disk and sync to all players (HOST only feature). " +
            "Usage: /chalkyloadboard [name] [index]. " +
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
                    "Interact with a board first, or pass an explicit index. e.g. /chalkyloadboard rat_king 1");
                return;
            }

            BoardIO.LoadBoard(index, name);
        }
    }
}
