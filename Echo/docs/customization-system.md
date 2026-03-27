# Customization System

## Overview

Player customization is stored in a **single flat struct** (`CustomizationDataIDs2`) with 18 fields. The split between "Appearance" and "Outfit" is logical only — the same struct holds both, and the game uses the `CustomizationType` enum to route lookups.

## `CustomizationType` enum

```csharp
public enum CustomizationType { Appearance, Outfit }
```

Used as a key when calling `CustomizationSettings.GetID(type, groupIndex, ...)` and `GetIndex(...)`.

## Appearance fields (`AppearanceType` enum, indices 0–6)

Face and body customization. Stored in `CustomizationData` / `CustomizationDataIDs2`:

| Field | `AppearanceType` | Note |
|-------|-----------------|------|
| `HeadData` | `Head` (0) | Mesh-based |
| `HairData` | `Hair` (1) | Mesh-based |
| `MustacheData` | `Mustache` (2) | Mesh-based |
| `EyeData` | `Eye` (3) | Sprite-based |
| `EyebrowData` | `Eyebrow` (4) | Sprite-based |
| `MouthData` | `Mouth` (5) | Sprite-based |
| `FacialData` | `Facial` (6) | Sprite-based |

## Outfit fields (`OutfitType` enum, indices 0–10)

Clothing and accessories. Also stored in the same `CustomizationDataIDs2`:

| Field | `OutfitType` | Index |
|-------|-------------|-------|
| `HatsData` | `Hats` | 0 |
| `EyeGlass` | `EyeGlass` | 1 |
| `Top` | `Top` | 2 |
| `Bottom` | `Bottom` | 3 |
| `Hands` | `Hands` | 4 |
| `Feet` | `Feet` | 5 |
| `Shoes` | `Shoes` | 6 |
| `Backpack` | `Backpack` | 7 |
| `Tail` | `Tail` | 8 |
| `FullBody` | `FullBody` | 9 |
| `Umbrella` | `Umbrella` | 10 |

`IsFullBody: bool` — when true, hides `Top`/`Bottom` in favor of `FullBody`.

## Style Slots (3 per player)

`PlayerDataZip` holds three full `CustomizationDataIDs2` structs:

```csharp
public CustomizationDataIDs2 CustomizationDataIDsNew;   // slot 0
public CustomizationDataIDs2 CustomizationDataIDs1New;  // slot 1
public CustomizationDataIDs2 CustomizationDataIDs2New;  // slot 2
public int SelectedStyleIndex;                          // active slot (0–2)
public List<string> StyleNames;                         // display names for each slot
```

The `CurrentCustomizationDataIDs` property gets/sets the struct for the active slot.

## Data flow

```
PlayerDataZip.CurrentCustomizationDataIDs   (persisted IDs — integers)
    ↕  CustomizationDataIDs2(CustomizationData) / new CustomizationData(ids)
DataManager.CustomizationData               (in-memory indices for rendering)
    ↓  ApplyCustomization(data)
TextChannelManager.MainCustomizationController   (network-synced visual state)
```

On slot switch, the game saves the current slot first, updates `SelectedStyleIndex`, then reloads `CustomizationData` from the new slot.

## Partial copy (outfit-only or appearance-only)

The game has no built-in partial-copy method. To copy only outfit fields from slot A to slot B, read both `CustomizationDataIDs2` structs and copy the 11 outfit fields manually while leaving the 7 appearance fields from the target untouched. The `CustomizationDataIDs2` struct is a value type, so assignment is a copy.

## Applying changes from a mod

```csharp
var dm = MonoSingleton<DataManager>.I;
var pdz = dm.PlayerDataZip;

// Write to the active slot
pdz.CurrentCustomizationDataIDs = updatedIds;

// Refresh in-memory and network
dm.CustomizationData = new CustomizationData(pdz.CurrentCustomizationDataIDs);
NetworkSingleton<TextChannelManager>.I.MainCustomizationController
    .ApplyCustomization(dm.CustomizationData);

dm.SavePlayerZipData();
```

## Color palettes

`CustomizationSettings` separates colors by domain:
- `_bodyColors` — head, facial, tail
- `_hairColors` — hair, mustache
- `_outfitColors` — all other outfit slots
