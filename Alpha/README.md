# Alpha

A shared BepInEx mod for **On Together** that provides common utilities and a command framework for other mods to build on.

- **Author:** AndrewLin
- **Repository:** https://github.com/andrewlimforfun/ot-mods

---

## Workaround: On-Together GPU VRAM & RAM Usage

On-Together accumulates GPU VRAM and system RAM over time as Unity loads assets during a session. If you experience frame drops or out-of-memory issues, run this command in chat to immediately free unused assets:

```
/auua
```

This calls `UnityEngine.Resources.UnloadUnusedAssets()` and can reclaim significant memory without restarting the game. Run it whenever the game feels sluggish or after leaving a busy lobby.

---

## Configuration

Alpha creates a config file at `BepInEx/config/com.andrewlin.ontogether.alpha.cfg`.

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| `Logging` | `TimestampLog` | `true` | Prepend `[HH:mm:ss]` timestamps to each line in `LogOutput.log`. |

---

## In-Game Commands

Alpha registers its own `/alpha` namespace commands. Type them in chat — they are intercepted locally and not sent to other players.

| Command | Short | Description |
|---|---|---|
| `/alphaserverinfo` | `/asi` | Show lobby name, code, player count, and host info |
| `/alphawhois [player]` | `/awi` | Show info about a player (name, Steam ID, position) |
| `/alphamyposition` | `/amp` | Show your current world position |
| `/alphaaddnotification <message>` | `/aan` | Post a local notification to your own chat |
| `/alphaunloadunusedassets` | `/auua` | Free GPU VRAM & RAM by unloading unused Unity assets |
| `/alphahelp` | `/ah` | List all Alpha commands |

---

## Features

### Chat Command Framework

Alpha exposes a static `AlphaPlugin.CommandManager` (`ChatCommandManager`) that any mod can use to register in-game slash commands.

**Implementing a command:**

```csharp
public class MyCommand : IChatCommand
{
    public string Name        => "mycmd";
    public string ShortName   => "mc";          // optional short alias
    public string Description => "Does something cool.";
    public string Namespace   => "mymod";        // used for grouping and auto-help

    public void Execute(string[] args) { /* ... */ }
}
```

**Registering in your plugin's `Awake()`:**

```csharp
AlphaPlugin.CommandManager?.Register(new MyCommand());
```

When the first command for a namespace is registered, a `/{namespace}help` command (and short form `/{ns[0]}h`) is automatically created for that namespace.

**Built-in help usage:**
```
/{mymod}help
/{mymod}help verbose       — includes descriptions
/{mymod}help mycmd         — looks up a single command
```

---

### Chat Utilities (`ChatUtils`)

| Method | Description |
|--------|-------------|
| `AddGlobalNotification(text)` | Displays a notification in the in-game global chat. |
| `SendMessageAsync(userName, text, isLocal)` | Sends a chat message (truncated to 250 chars). |
| `SendChunkedMessageAsync(userName, text, isLocal, strategy)` | Splits long text into ≤250-char chunks and sends each one. Strategy: `"word"` (default) or `"hard"`. |
| `CleanTMPTags(input)` | Strips TextMeshPro formatting tags (e.g. `<#ff0000>`, `<b>`) from a string. |
| `CleanCommand()` | Hides a slash command from chat after it is processed (clears the input field). |
| `UISendMessage(text)` | Injects text into the chat input field and submits it programmatically. |

---

### Time Utilities (`TimeUtils`)

| Method | Description |
|--------|-------------|
| `TryParseDuration(input, out TimeSpan result)` | Parses ISO 8601 durations (`1h30m`, `15s`, `PT1H30M`) and `hh:mm:ss` / `TimeSpan` strings. Returns `false` if unparseable. |

**Example:**
```csharp
if (TimeUtils.TryParseDuration("10m", out TimeSpan duration))
    // duration == TimeSpan.FromMinutes(10)
```

---

### String Chunking (`IStringChunker`)

Breaks arbitrary text into segments no longer than a given character limit.

| Implementation | Behaviour |
|----------------|-----------|
| `WordBoundaryChunker` | Splits on whitespace; avoids cutting words mid-word. |
| `HardCutChunker` | Hard-cuts at the character limit, no word awareness. |

---

### Player Utilities (`PlayerUtils`)

| Method | Description |
|--------|-------------|
| `GetUserName()` | Returns the local player's display name (may contain TMP tags). |
| `GetUserNameNoFormat()` | Returns the display name with TMP tags stripped. |

---

### Main-Thread Dispatch

```csharp
AlphaPlugin.RunOnMainThread(() => { /* runs on Unity main thread next Update */ });
```

Useful for scheduling Unity API calls from background threads (e.g. WebSocket handlers).

---

## Configuration

All settings are managed by BepInEx and can be changed in `BepInEx/config/com.andrewlin.ontogether.alpha.cfg`.

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| General | `EnableFeature` | `true` | Master enable/disable switch for Alpha. |
| General | `ShowCommand` | `false` | Keep the slash command visible in chat after execution. |

---

## Installation

Install via [Thunderstore](https://thunderstore.io) / r2modman, or drop the `.dll` into `BepInEx/plugins/`.

**Dependency:** `BepInEx-BepInExPack-5.4.2305`

---

## License

MIT
