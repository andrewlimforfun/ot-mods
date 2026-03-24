using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using PurrNet;
using UnityEngine;
using Alpha.Core.Util;

namespace Chalky.Core
{
    /// <summary>
    /// Handles saving and loading chalkboard state to/from disk, and broadcasting
    /// the loaded state to all connected players.
    ///
    /// Save format: &lt;name&gt;.chalkboard.json (JSON int[][] grid) + &lt;name&gt;.chalkboard.png (optional preview)
    /// Grid layout: [x][y] where x=column (0..319), y=row (0..179), value=0 means empty,
    /// value n means chalk color index (n-1) via DrawingManager.GetColor.
    /// </summary>
    public static class BoardIO
    {
        private readonly static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(ChalkyPlugin.ModName);

        // ----
        // Public API
        // ----

        /// <summary>
        /// Lists all saved chalkboards in the save directory, returning just the base names without extensions.
        /// </summary>        
        public static List<string> ListSavedBoards()
        {
            if (!Directory.Exists(ChalkyPlugin.SaveDir))
            {
                Logger.LogWarning($"[BoardIO] Save directory not found: {ChalkyPlugin.SaveDir}");
                return new List<string>();
            }

            var files = Directory.GetFiles(ChalkyPlugin.SaveDir, "*.chalkboard.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f).Replace(".chalkboard", "")).ToList();
        }

        /// <summary>
        /// Resolves which board index to target.
        /// If <paramref name="indexArg"/> is a valid integer, use it.
        /// Otherwise fall back to the currently active board (_boardIndex on DrawingManager).
        /// Returns -1 and logs an error if no valid board is found.
        /// </summary>
        public static int ResolveBoard(string? indexArg)
        {
            var dm = MonoSingleton<DrawingManager>.I;
            if (dm == null)
            {
                Logger.LogError("[BoardIO] DrawingManager not found.");
                return -1;
            }

            int index;
            if (!string.IsNullOrEmpty(indexArg) && int.TryParse(indexArg, out int parsed))
            {
                index = parsed;
            }
            else
            {
                // Fall back to active board tracked by DrawingManager
                index = (int)AccessTools.Field(typeof(DrawingManager), "_boardIndex").GetValue(dm);
            }

            if (index < 0 || index >= dm.QuadPainterGPUS.Count)
            {
                Logger.LogError(
                    $"[BoardIO] Invalid board index {index}. " +
                    $"Valid range: 0–{dm.QuadPainterGPUS.Count - 1}. " +
                    "Interact with a board first, or pass an explicit index.");
                return -1;
            }

            return index;
        }

        /// <summary>
        /// Saves board at <paramref name="index"/> to &lt;SaveDir&gt;/&lt;name&gt;.chalkboard.json
        /// and an optional &lt;name&gt;.chalkboard.png preview. Anyone (host or client) can call this.
        /// </summary>
        public static void SaveBoard(int index, string name)
        {
            var dm = MonoSingleton<DrawingManager>.I;
            if (dm == null || index < 0 || index >= dm.QuadPainterGPUS.Count)
            {
                Logger.LogError($"[BoardIO] SaveBoard: invalid state or index {index}");
                return;
            }

            var board = dm.QuadPainterGPUS[index];

            // Extract int[][] from IntList[] -- avoids any IntList serialization quirks
            int[][] grid = board.PaintColors
                .Select(col => col.Ints.ToArray())
                .ToArray();

            string boardPath = Path.Combine(ChalkyPlugin.SaveDir, name + ".chalkboard.json");
            File.WriteAllText(boardPath, JsonConvert.SerializeObject(grid));
            Logger.LogInfo($"[BoardIO] Board {index} saved to {boardPath}");

            SavePreview(board, name);
        }

        /// <summary>
        /// Loads a previously saved board from disk onto board at <paramref name="index"/>,
        /// rebuilds the local render texture, and broadcasts the new state to all
        /// currently connected players. Late-joiners get it automatically via the
        /// existing ReadPixelData => GetQuadImage flow.
        /// Requires you to be the host (board.isServer == true).
        /// </summary>
        public static void LoadBoard(int index, string name)
        {
            var dm = MonoSingleton<DrawingManager>.I;
            if (dm == null || index < 0 || index >= dm.QuadPainterGPUS.Count)
            {
                Logger.LogError($"[BoardIO] LoadBoard: invalid state or index {index}");
                return;
            }

            var board = dm.QuadPainterGPUS[index];

            if (!board.isServer)
            {
                ChatUtils.AddGlobalNotification("Relaying board to other players via the host...");
            }

            string boardPath = Path.Combine(ChalkyPlugin.SaveDir, name + ".chalkboard.json");
            if (!File.Exists(boardPath))
            {
                ChatUtils.AddGlobalNotification($"Board file not found: {name}.chalkboard.json");
                Logger.LogError($"[BoardIO] File not found: {boardPath}");
                return;
            }

            // Deserialize int[][] and write back into PaintColors.Ints
            int[][] grid = JsonConvert.DeserializeObject<int[][]>(File.ReadAllText(boardPath))!;
            for (int x = 0; x < board.PaintColors.Length && x < grid.Length; x++)
            {
                for (int y = 0; y < board.PaintColors[x].Ints.Count && y < grid[x].Length; y++)
                {
                    board.PaintColors[x].Ints[y] = grid[x][y];
                }
            }

            Logger.LogInfo($"[BoardIO] PaintColors restored from {boardPath}");

            RebuildRenderTexture(board, dm);
            BroadcastToPlayers(board);

            Logger.LogInfo($"[BoardIO] Board {index} loaded and synced to all players.");
        }

        // ----
        // Internals
        // ----

        private static void SavePreview(QuadPainterGPU board, string name)
        {
            var rt = Traverse.Create(board).Field("_rt").GetValue<RenderTexture>();
            if (rt == null)
            {
                Logger.LogWarning("[BoardIO] _rt is null - skipping PNG preview.");
                return;
            }

            var tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;

            string pngPath = Path.Combine(ChalkyPlugin.SaveDir, name + ".chalkboard.png");
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            Object.Destroy(tex);

            Logger.LogInfo($"[BoardIO] Preview saved to {pngPath}");
        }

        private static void RebuildRenderTexture(QuadPainterGPU board, DrawingManager dm)
        {
            var traverse = Traverse.Create(board);
            var rt = traverse.Field("_rt").GetValue<RenderTexture>();
            if (rt == null)
            {
                Logger.LogError("[BoardIO] _rt is null - cannot rebuild render texture.");
                return;
            }

            // Clear the existing canvas
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(clearDepth: true, clearColor: true, Color.clear);
            RenderTexture.active = prev;

            // Re-paint every non-empty cell into the pixel batch
            // PaintPixel signature: private void PaintPixel(Vector2Int coord, Color color, int colorIndex = -1)
            var paintPixel = traverse.Method("PaintPixel",
                new[] { typeof(Vector2Int), typeof(Color), typeof(int) });

            for (int x = 0; x < board.PaintColors.Length; x++)
            {
                for (int y = 0; y < board.PaintColors[x].Ints.Count; y++)
                {
                    int colorIdx = board.PaintColors[x].Ints[y];
                    if (colorIdx != 0)
                    {
                        Color color = dm.GetColor(colorIdx - 1);
                        paintPixel.GetValue(new Vector2Int(x, y), color, colorIdx);
                    }
                }
            }

            // Flush the accumulated pixel batch to the RenderTexture
            traverse.Method("RenderBatch").GetValue();

            Logger.LogInfo("[BoardIO] RenderTexture rebuilt from PaintColors.");
        }

        private static void BroadcastToPlayers(QuadPainterGPU board)
        {
            var playerIDs = NetworkSingleton<PlayerPanelController>.I.PlayerIDs;

            if (board.isServer)
            {
                // Host path: send directly to all players (server can target anyone)
                int count = 0;
                foreach (var player in playerIDs)
                {
                    board.GetQuadImage(player, board.PaintColors);
                    count++;
                }
                Logger.LogInfo($"[BoardIO] Broadcast to {count} player(s).");
            }
            else
            {
                // Non-host path: send to the host only. The host-side postfix
                // on GetQuadImage_Original_1 will relay to all other players.
                PlayerID? hostPlayer = null;
                foreach (var p in playerIDs)
                {
                    if (p.isServer) { hostPlayer = p; break; }
                }

                if (hostPlayer == null)
                {
                    Logger.LogWarning("[BoardIO] Could not find host player in PlayerIDs. Board not broadcast.");
                    ChatUtils.AddGlobalNotification("Could not find host player to relay board.");
                    return;
                }

                board.GetQuadImage(hostPlayer.Value, board.PaintColors);
                Logger.LogInfo("[BoardIO] Sent board state to host for relay.");
            }
        }
    }
}
