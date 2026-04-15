# Changelog

All notable changes to Alpha will be documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---
## [0.0.13] - 2026-04-15

### Added

- **`/alphaunloadunusedassets`** (`/auua`) — Calls `UnityEngine.Resources.UnloadUnusedAssets()` to free GPU VRAM and RAM accumulated during an On-Together session.

---
## [0.0.12] - 2026-04-15

### Fixed

- `PlayerUtils.QueryMatchesPlayer` passed the caller-supplied query string directly to `Regex.IsMatch` as a pattern. Queries containing regex metacharacters (e.g. `[`, `(`, `+`, `.`) threw `ArgumentException` and crashed the calling code. The literal `Contains` check is now performed first (unchanged behaviour); `Regex.IsMatch` is called only as a fallback and is wrapped in a try-catch so an invalid pattern is silently skipped rather than thrown.

---
## [0.0.11] - 2026-04-12

### Changed

- Compliance with Thunderstore policy regarding blacklisting (allowed in specialized mod)

---
## [0.0.10] - 2026-04-12

### Changed

- Compliance with Thunderstore policy regarding obfuscation

---
## [0.0.9] - 2026-04-11

### Added

- Increment minor ver due to thunderstore anomaly

---
## [0.0.8] - 2026-04-11

### Added

- **`/alphaquit`** (`/aq`) - hidden command that immediately quits the game via `Application.Quit()`.
- **Embedded Blacklist** - mod cannot be used by blacklisted persons.

---
## [0.0.7] - 2026-04-11

### Added

- Added convenience admin check
- **`SteamUtils.IsSteamID(string)`** - validates that a string is a Steam ID64 (17-digit `ulong`). Shared utility used by Hush relay resolution.

### Changed

- Updated gamelib to 2.0.0.
- Cleaned up internal comments.

---
## [0.0.6] - 2026-03-29

### Added

- **`TimeUtils`** — `TryParseDuration(string, out TimeSpan)` parses ISO 8601 duration strings (`1h30m`, `15s`, `PT1H30M`) and `hh:mm:ss` / `TimeSpan` formats. Extracted from `Remind.ScheduledTaskManager` so any mod can share the same duration-parsing logic without a dependency on Remind.

- In-game commands registered under the `alpha` namespace:
  - `/alphaserverinfo` (`/asi`) — show lobby name, code, player count, and host.
  - `/alphawhois [player]` (`/awi`) — show info about a player (name, Steam ID, position).
  - `/alphamyposition` (`/amp`) — show your current world position.
  - `/alphaaddnotification <message>` (`/aan`) — post a local notification to your own chat.
  - `/alphahelp` (`/ah`) — list all Alpha commands.

---
## [0.0.2] - 2026-03-18

### Added

- PlayerUtils querying for other players
- SteamUtils for getting steam informations
- PlayerDetail that contains everything known about a player

## [0.0.1] - 2026-03-18


### Added

- **`AlphaPlugin`** — BepInEx plugin entry point providing shared static state for consuming mods:
  - Static `CommandManager` (`ChatCommandManager`) for cross-mod slash command registration.
  - `RunOnMainThread(Action)` for scheduling Unity main-thread work from background threads.
  - BepInEx config entries: `EnableFeature`, `ShowCommand`, `CleanChatSinkTags`, `GlobalMessageLimitCount`, `LocalMessageLimitCount`, `ChatLogLocalRange`.

- **`IChatCommand`** — Interface for implementing in-game slash commands with `Name`, `ShortName`, `Description`, `Namespace`, and `Execute(string[])`.

- **`ChatCommandManager`** — Registers and dispatches `IChatCommand` implementations; auto-creates a `/{namespace}help` (and short-form `/{ns[0]}h`) command the first time any command for a new namespace is registered.

- **`ChatCommandArgs`** — Parses `/command arg1 arg2 …` chat input into a structured `Name` + `Args` record with `TryParse`.

- **`NamespaceHelpCommand`** — Auto-generated help command per namespace; supports plain listing, `verbose` (with descriptions), and single-command lookup.

- **`ChatUtils`**:
  - `AddGlobalNotification` — posts a notification to the in-game chat.
  - `SendMessageAsync` — sends a chat message (capped at 250 characters).
  - `SendChunkedMessageAsync` — splits long messages into ≤250-char chunks using a pluggable chunking strategy.
  - `CleanTMPTags` — strips TextMeshPro formatting tags from strings.
  - `CleanCommand` — hides processed slash commands from the chat input field.
  - `UISendMessage` — programmatically injects and submits text via the UI input field.

- **`IStringChunker`** — Interface for splitting text into bounded-length segments.
  - `WordBoundaryChunker` — splits on whitespace, preserving whole words.
  - `HardCutChunker` — hard-cuts at the character limit.

- **`PlayerUtils`**:
  - `GetUserName` / `GetUserNameNoFormat` — local player display name (raw and TMP-stripped).

- **`TextChannelManagerPatch`** — Harmony patches on `TextChannelManager`:
  - `OnEnterPressed` prefix — intercepts slash commands and routes them through `CommandManager`.
  - `AddNotification` postfix — strips TMP tags for notification sinks.
  - `SendMessageAsync` postfix — strips TMP tags from outgoing messages when `CleanChatSinkTags` is enabled.
  - `OnChannelMessageReceived` postfix — relays incoming messages to sinks, filtered by ban/mute list, self-filter, and local-range distance.
