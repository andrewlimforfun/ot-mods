# BroadcastToPlayers Fix - Host Relay for Non-Host Clients

## Problem

When a non-host client calls `BoardIO.LoadBoard`, only the host and the calling
client see the loaded board. All other connected players do not receive the
update and must rejoin to see it.

## Root Cause

`GetQuadImage` is declared as:

```csharp
[TargetRpc(Channel.ReliableOrdered, requireServer: true)]
public void GetQuadImage(PlayerID targetID, IntList[] paintedColors, ...)
```

The `requireServer: true` flag means **only the server/host** can send this RPC.

### What Happens When the Host Calls It

```
board.GetQuadImage(playerX, PaintColors)
  → SendRPCNormal
    → isServer == true
    → BatchToTargets(playerX, ...)    // direct send - always works
```

All players receive it. No issues.

### What Happens When a Non-Host Client Calls It

```
board.GetQuadImage(playerX, PaintColors)
  → SendRPCNormal
    → isServer == false
    → ValidateSendingRPC checks:
      requireServer: true && !networkManager.isServer
      → LogError("Trying to send RPC 'GetQuadImage' without server.")
      → return false - PACKET DROPPED
```

The RPC never leaves the client. The only exception is when `targetID` equals
`localPlayerForced` - in that case, the code skips `SendRPC` entirely and calls
`GetQuadImage_Original_1` locally. This is why the caller always sees their own
load succeed.

### Why Self + Host Worked

The original `BroadcastToPlayers` iterated all `PlayerIDs`:

```csharp
foreach (var player in playerIDs)
    board.GetQuadImage(player, board.PaintColors);
```

- **Self** (`player == localPlayerForced`): Skips the network path, runs
  `GetQuadImage_Original_1` locally. Works.
- **Host** (`player.isServer`): For non-host callers, the TargetRpc packet is
  blocked by `ValidateSendingRPC`. However since `requireServer: true` blocks
  the send, the host never receives it either - unless `ignoreRequireServer` is
  enabled in NetworkRules.
- **Other clients**: Same block. `ValidateSendingRPC` drops the packet.

Result: only the local client sees the board.

## Solution: Host Relay Pattern

### Strategy

1. **Non-host client** sends `GetQuadImage` only to the **host** (the one
   player where the TargetRpc might succeed, given game configuration).
2. **Host** receives the board data via `GetQuadImage_Original_1`, applying it
   locally.
3. A **Harmony postfix** on `GetQuadImage_Original_1` detects: "I'm the host,
   and this data came from a non-host player" → fans out `GetQuadImage` to all
   other players using the server's direct `BatchToTargets` path (which always
   works).

### Changes

#### `BoardIO.BroadcastToPlayers` (BoardIO.cs)

```csharp
if (board.isServer)
{
    // Host: send to all players directly
    foreach (var player in playerIDs)
        board.GetQuadImage(player, board.PaintColors);
}
else
{
    // Non-host: send only to host, who will relay
    var hostPlayer = playerIDs.First(p => p.isServer);
    board.GetQuadImage(hostPlayer, board.PaintColors);
}
```

#### `QuadPainterGPUPatch.RelayGetQuadImage` (QuadPainterGPUPatch.cs)

```csharp
[HarmonyPatch("GetQuadImage_Original_1")]
[HarmonyPostfix]
public static void RelayGetQuadImage(QuadPainterGPU __instance, RPCInfo rpcInfo)
{
    if (!__instance.isServer) return;             // only host relays
    var sender = rpcInfo.sender;
    if (sender == __instance.localPlayerForced) return;  // host's own load
    if (sender == default(PlayerID)) return;             // local call, no relay

    foreach (var player in PlayerIDs)
    {
        if (player == sender) continue;                  // sender already has it
        if (player == __instance.localPlayerForced) continue; // host already applied
        __instance.GetQuadImage(player, __instance.PaintColors);
    }
}
```

### Why This Is Safe

- **No infinite relay**: The postfix only fires when `isServer == true` AND
  `sender != localPlayerForced`. When the host sends the relayed `GetQuadImage`
  to other clients, those clients have `isServer == false` → the postfix
  returns immediately.
- **No double-application on host**: The host's `GetQuadImage_Original_1`
  already applied the pixels before the postfix runs. The postfix only sends
  to *other* players.
- **No double-send to sender**: The postfix explicitly skips `player == sender`.
- **Host's own LoadBoard still works**: When the host calls LoadBoard, the
  `rpcInfo` is default-constructed (local call, never went through network).
  `rpcInfo.sender == default(PlayerID)` → postfix returns immediately.
  The host's `BroadcastToPlayers` takes the `isServer` branch and sends
  directly to all players.

### Limitations

- **Requires Chalky mod on the host**: The Harmony postfix that does the relay
  only exists on modded clients. If the host doesn't have Chalky installed,
  non-host board loads will only be visible locally.
- **Payload size**: `GetQuadImage` sends the full `IntList[]` (320×180 ints).
  The relay means the data traverses the network twice (client→host, host→each
  player). This is acceptable for a one-time load operation.
