using Chalky.Core;
using Alpha.Core.Util;
using Alpha.Core.Command;

namespace Chalky.Core.Commands
{
    /// <summary>
    /// /chalkygetboards
    ///
    /// Lists all saved chalkboards.
    /// </summary>
    public class ChalkyGetBoardsCommand : IChatCommand
    {
        public const string CMD = "chalkygetboards";
        public string Name => CMD;
        public string ShortName => "cgb";
        public string Description =>
            "List all saved chalkboards. Usage: /chalkygetboards.";

        public string Namespace => "chalky";
        public void Execute(string[] args)
        {
            var boards = BoardIO.ListSavedBoards();
            if (boards.Count == 0)
            {
                ChatUtils.AddGlobalNotification("No saved chalkboards found.");
                return;
            }

            string boardList = string.Join(", ", boards);
            ChatUtils.AddGlobalNotification($"Saved chalkboards: {boardList}");
        }
    }
}
