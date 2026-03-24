# Chalkboard Networking Internals

> Reference doc for how On Together's chalkboard drawing and board-sync systems
> work under the hood, with PurrNet RPC semantics.

---

## PurrNet RPC Primer

PurrNet (the Unity networking layer) uses three RPC types, each with key flags:

| RPC Type | Direction | Key Flags |
|---|---|---|
| `[ServerRpc]` | Client → Server | `requireOwnership` (default `true`) |
| `[ObserversRpc]` | Server → All Observers | `requireServer` (default `true`) |
| `[TargetRpc]` | Server → One Client | `requireServer` (default `true`) |

### `requireServer: true` (default)

- **Sending**: `ValidateSendingRPC` blocks the packet on non-server clients with a
  `LogError("...without server.")`. The packet never leaves the client.
- **Server receive**: `ValidateIncomingRPC` rejects client-originated packets,
  logging `"If you want automatic forwarding use 'requireServer: false'."`.

### `requireServer: false`

- **Sending**: Non-server clients may send. The packet goes to the server.
- **Server receive**: The server **automatically relays** to all observers
  (ObserversRpc) or the specific target (TargetRpc). This is the "client relay"
  pattern.

### `NetworkRules` Override

A `NetworkRules` ScriptableObject on the `NetworkManager` can override these
checks globally:

```
ignoreRequireServerAttribute = true  →  ShouldIgnoreRequireServer() = true
ignoreRequireOwnerAttribute  = true  →  ShouldIgnoreRequireOwner()  = true
targetRpcsCanTargetServer    = true  →  CanTargetServerWithTargetRpc() = true
```

If `ignoreRequireServerAttribute` is enabled, the `requireServer` flag on all
RPCs is effectively treated as `false`—clients can send ObserversRpc and
TargetRpc, and the server will relay them.

### Send Path (from `SendRPCNormal`)

```
ObserversRPC:
  if isServer → BatchToTargets(observers, ...)     // direct fan-out
  else        → BatchToServer(...)                  // client: send to server for relay

TargetRPC:
  if isServer → BatchToTargets(targets, ...)        // direct send to target(s)
  else        → for each target: BatchToServer(packet { targetPlayerId = target })

ServerRPC:
  → BatchToServer(...)                              // always to server
```

### Server Relay Path (from `ValidateIncomingRPC` when `asServer == true`)

```
ObserversRPC (requireServer: false or ignoreRequireServer):
  → Send(observers - sender/owner as needed, ...)
  → return !isClient  (host = don't execute locally; pure server = execute)

TargetRPC (requireServer: false or ignoreRequireServer):
  → SendToTargetOrServer(targetPlayerId, ...)
  → returns true only if target == PlayerID.Server && targetRpcsCanTargetServer
```

---

## QuadPainterGPU — Key Fields

| Field | Type | Purpose |
|---|---|---|
| `PaintColors` | `IntList[]` | Grid of color indices. `PaintColors[x].Ints[y]` = 0 (empty) or `n` where color = `GetColor(n-1)`. Grid: 320×180. **Not a SyncVar**—synced only via RPC snapshots. |
| `gridSize` | `Vector2Int` | Default `(320, 180)` |
| `textureWidth/Height` | `int` | Default `1920 × 1080` |
| `_rt` | `RenderTexture` (private) | The actual drawn canvas |
| `_pixelsToUpdate` | `Dictionary<Vector2Int, Color>` (private) | Batch of pixels queued for GPU render |
| `previousUV` | `Vector2` (private) | Last mouse UV for stroke interpolation (`-1` = no prev) |
| `IsActive` | `bool` | Whether this board is currently being drawn on by the local player |

`IntList` is a simple wrapper: `public class IntList { public List<int> Ints; }`,
initialized with a given capacity.

---

## QuadPainterGPU — RPCs

### 1. `ReadPixelData` — Board Sync Request

```
[ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
```

| Property | Value |
|---|---|
| Direction | Client → Server |
| `requireOwnership` | `true` |
| Called by | `DrawingManager.SyncBoards()` on each board (1s stagger) |
| Trigger | Non-host client joins (`PlayerCustomizationController.OnSpawned`) |

**Server-side handler** (`ReadPixelData_Original_0`):
```csharp
GetQuadImage(rpcInfo.sender, PaintColors);  // send board back to requester
```

### 2. `GetQuadImage` — Full Board State Push

```
[TargetRpc(Channel.ReliableOrdered, requireServer: true)]
```

| Property | Value |
|---|---|
| Direction | Server → One Client |
| `requireServer` | **`true`** |
| `requireOwnership` | `false` |
| Payload | `PlayerID targetID, IntList[] paintedColors` |

**Receiver handler** (`GetQuadImage_Original_1`):
```csharp
PaintColors = paintedColors;
// Re-render every non-empty cell
for x, y: if paintedColors[x][y] != 0 → PaintPixel(coord, color, idx)
RenderBatch();
```

### 3. `FillTheBlanksRPC` — Live Paint Stroke Broadcast

```
[ObserversRpc(Channel.ReliableOrdered, requireServer: true)]
```

| Property | Value |
|---|---|
| Direction | Server → All Observers |
| `requireServer` | **`true`** |
| `runLocally` | `false` |
| `excludeSender` | `false` |
| Payload | `Vector2 uv, Vector2 prevUV, int colIndex, bool isErase, bool isBigErase` |

**Receiver handler** (`FillTheBlanksRPC_Original_2`):
```csharp
if (rpcInfo.sender != localPlayerForced)
    FillTheBlanks(uv, prevUV, colIndex, isErase, isBigErase, isSelf: false);
```
The sender check prevents double-rendering (local player already called
`FillTheBlanks(..., isSelf: true)` directly in `Update()`).

---

## Call Sequences

### Normal Drawing (Host)

```
QuadPainterGPU.Update()
  │ Raycast hit → textureCoord
  ├─ FillTheBlanksRPC(uv, prev, ...)       [ObserversRpc: server sends to all]
  │    → ValidateSendingRPC → OK (isServer)
  │    → BatchToTargets(observers)
  │    → Each observer: FillTheBlanksRPC_Original_2
  │         if sender != localPlayer → FillTheBlanks(..., isSelf: false)
  └─ FillTheBlanks(uv, prev, ..., isSelf: true)  [local render immediately]
       → PaintOnTexture → TryPaint → PaintPixel → _pixelsToUpdate[coord]=color
       → LateUpdate() → RenderBatch()
```

### Normal Drawing (Non-Host Client)

> Whether this works depends on `NetworkRules.ignoreRequireServerAttribute`.
> If the game has this set to `true`, the client's `FillTheBlanksRPC` gets
> relayed through the server to all observers. If `false` (default), the RPC
> is silently blocked and only the local player sees the stroke.

```
QuadPainterGPU.Update()
  │ Raycast hit → textureCoord
  ├─ FillTheBlanksRPC(uv, prev, ...)
  │    → ValidateSendingRPC
  │      if ignoreRequireServer:
  │         → BatchToServer(...)
  │         → Server.ValidateIncomingRPC → OK
  │         → Server forwards to all observers
  │      else:
  │         → LogError("...without server.") → BLOCKED
  └─ FillTheBlanks(uv, prev, ..., isSelf: true)  [always runs locally]
```

### Late-Joiner Board Sync

```
PlayerCustomizationController.OnSpawned()  [non-host only, isOwner check]
  └─ StartCoroutine(DrawingManager.SyncBoards())
       └─ for each board:
            board.ReadPixelData()              [ServerRpc → server]
              → Server: ReadPixelData_Original_0(rpcInfo)
                → GetQuadImage(rpcInfo.sender, PaintColors)  [TargetRpc → requester]
                  → Client: GetQuadImage_Original_1
                    → PaintColors = paintedColors
                    → repaint + RenderBatch()
            yield WaitForSeconds(1)
```

### Board Load — Host

```
BoardIO.LoadBoard(index, name)
  ├─ Deserialize grid from JSON → write into board.PaintColors
  ├─ RebuildRenderTexture(board, dm)
  │    → GL.Clear → PaintPixel each cell → RenderBatch()
  └─ BroadcastToPlayers(board)
       └─ foreach player in PlayerIDs:
            board.GetQuadImage(player, board.PaintColors)  [TargetRpc: direct]
```

### Board Load — Non-Host Client (Chalky Mod Workaround)

```
BoardIO.LoadBoard(index, name)
  ├─ Deserialize grid → write into board.PaintColors
  ├─ RebuildRenderTexture (local only)
  └─ BroadcastToPlayers(board)
       └─ board.GetQuadImage(hostPlayer, board.PaintColors)
            → TargetRpc sent to server
            → Server: GetQuadImage_Original_1 runs (applies pixels locally)
            → Harmony Postfix: RelayGetQuadImage triggers
              → Detects: isServer=true, sender != self, sender != default
              → foreach other player: GetQuadImage(player, PaintColors)
                [TargetRpc: server direct → BatchToTargets → works]
```

---

## The BroadcastToPlayers Problem & Solution

See [broadcast-fix.md](broadcast-fix.md) for the full investigation.
