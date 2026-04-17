# Sweep

Periodically unloads unused Unity assets to reclaim memory. Runs automatically every 30 minutes by default. This mod just calls `UnityEngine.Resources.UnloadUnusedAssets()`

## Commands

| Command | Short | Description |
|---------|-------|-------------|
| `/sweepinterval` | `/si` | Get or set the sweep interval (e.g. `30M`, `1H`, `PT1H30M`) |

## Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| Interval | 30M | How often to sweep. ISO 8601 duration, `PT` prefix optional |

## Installation

Install via [Thunderstore](https://thunderstore.io/c/on-together/) or r2modman.

Requires: [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)
