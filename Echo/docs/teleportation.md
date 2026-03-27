# Teleportation

## How it works

Direct assignment of `CharacterController.transform.position` is overwritten every frame by `PlayerMovementController.MovePlayer`. The correct approach (confirmed by `officer-balls/Teleport`) is to **queue the warp position and apply it from inside `MovePlayer`**, then skip that frame's movement.

## Implementation in Echo

### `PlayerMovementControllerPatch` (`Patches/PlayerMovementControllerPatch.cs`)

```csharp
public static Vector3 WarpPosition = Vector3.zero;

// HarmonyPrefix on PlayerMovementController.MovePlayer
// - If WarpPosition is non-zero and __instance is the local player's controller:
//   sets CharacterController.transform.position = WarpPosition, clears it, returns false (skip original)
// - Otherwise returns true (normal movement)
```

### `MoveUtil.Teleport(Vector3)` (`Core/MoveUtil.cs`)

```csharp
MoveUtil.Teleport(targetPosition);
```

1. Sets `PlayerMovementControllerPatch.WarpPosition = targetPosition`
2. Also sets `tcm.MainPlayer.position = targetPosition` immediately for network visibility

The patch applies the actual `CharacterController` move on the next `MovePlayer` tick.

## Key game references

| Symbol | Type | Purpose |
|--------|------|---------|
| `NetworkSingleton<TextChannelManager>.I.MainMovementController` | `PlayerMovementController` | Local player's movement controller |
| `NetworkSingleton<TextChannelManager>.I.MainPlayer` | `Transform` | Local player's root transform (network-synced) |
| `____characterController` | `CharacterController` | Harmony ref to private field in `PlayerMovementController` |

## Getting another player's position

```csharp
// PlayerPanelController holds all connected players
var panel = NetworkSingleton<PlayerPanelController>.I;
var idx = ...; // index into panel.PlayerIDs
var controller = panel.PlayerTransforms[idx]
    .GetComponentInChildren<PlayerMovementController>();
Vector3 pos = controller.transform.position;
```

Then pass `pos` to `MoveUtil.Teleport(pos)`.
