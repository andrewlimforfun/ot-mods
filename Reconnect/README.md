# Reconnect

Auto-reconnect to lobby after unexpected disconnection. Will not attempt reconnect on deliberate leaves (quit button, etc.). 

## Features

- Detects unexpected disconnection vs intentional leave
- Attempts up to 3 reconnects with configurable interval and max attempt
- Cooldown prevents infinite reconnect loops
- Falls back to normal menu return if all attempts fail
- Hosts are excluded (only clients reconnect)

## Disconnect behavior

### Intentional disconnect

An intentional disconnect is any action where the player explicitly chose to leave:

- Clicking the **Quit** button in the pause menu

**Sequence:** The game's normal flow runs unmodified. The client disconnects and returns to the main menu immediately. No reconnect is attempted.

### Unintentional disconnect

An unintentional disconnect is anything that triggers `ConnectionLost` without a prior intentional-leave marker:

- Host closes the game or crashes
- Network drop or timeout
- Host kicks the client

**Sequence:**
1. `ConnectionLost` fires on the client.
2. The patch suppresses the normal return-to-menu flow.
3. A notification is shown: "Connection lost - attempting to reconnect..."
4. The reconnect sequence starts. Up to `MaxAttempts` attempts are made, each waiting up to `AttemptIntervalSec` seconds for the connection to resolve.
5. On success: the client is connected again. The intentional-leave flag is cleared and the sequence ends silently.
6. On failure (all attempts exhausted, or `StartClient` throws every time): the normal menu-return flow is restored and the game returns to the main menu as if `ConnectionLost` had run normally.

A **cooldown** (`CooldownSec`) prevents a new reconnect sequence from starting too soon after the last one, avoiding rapid-fire loops if the host is unstable.

## Commands

| Command | Short | Description |
|---------|-------|-------------|
| `/reconnecttoggle` | `/rct` | Get or set auto-reconnect on/off. No arg toggles; `on`/`off` sets directly |
| `/reconnectmaxattempts` | `/rcma` | Get or set max reconnect attempts (1-100) |
| `/reconnectinterval` | `/rciv` | Get or set seconds between attempts (2-30) |
| `/reconnectcooldown` | `/rccd` | Get or set cooldown between sequences in seconds (10-120) |
| `/reconnectsimulate` | `/rcs` | Simulate an unintentional disconnect to test the reconnect sequence |
| `/reconnectfocus` | `/rcf` | Manually repair broken focus areas after reconnect |
| `/reconnecthelp` | `/rch` | List all reconnect commands |

## Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| Enabled | true | Enable auto-reconnect |
| MaxAttempts | 3 | Max reconnect attempts (1-100) |
| AttemptIntervalSec | 5 | Seconds between attempts |
| CooldownSec | 30 | Minimum seconds between reconnect sequences |

## Installation

Install via [Thunderstore](https://thunderstore.io/c/on-together/) or r2modman.

Requires: [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)
