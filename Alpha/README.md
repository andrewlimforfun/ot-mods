# Alpha

A shared BepInEx mod for **On Together** that provides common utilities and a command framework for other mods to build on.

- **Author:** AndrewLin
- **Version:** 0.0.1
- **Repository:** https://github.com/andrewlimforfun/on-together-mods

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
/mymodhelpcommand
/mymodhelp verbose       — includes descriptions
/mymodhelp mycmd         — looks up a single command
```

---

### Chat Utilities (`ChatUtils`)

| Method | Description |
|--------|-------------|
| `AddGlobalNotification(text)` | Displays a notification in the in-game global chat. |
| `SendMessageAsync(userName, text, isLocal)` | Sends a chat message (truncated to 250 chars). |
| `SendChunkedMessageAsync(userName, text, isLocal, strategy)` | Splits long text into ≤250-char chunks and sends each one. Strategy: `"word"` (default) or `"hard"`. |
| `CleanTMPTags(input)` | Strips TextMeshPro formatting tags (e.g. `<#ff0000>`, `<b>`) from a string. |
| `CleanCommand(helpCommand)` | Hides a slash command from chat after it is processed (clears the input field). |
| `UISendMessage(text)` | Injects text into the chat input field and submits it programmatically. |

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
| `GetSteamPlayerIdString()` | Returns the local player's Steam ID string (cached after first call). |
| `GetPlayerId()` | Returns the local player's `PlayerID`. |
| `GetPlayerIdForced()` | Returns the local player's forced `PlayerID`. |

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
