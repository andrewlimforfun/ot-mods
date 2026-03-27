# Changelog

All notable changes to Alpha will be documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

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
