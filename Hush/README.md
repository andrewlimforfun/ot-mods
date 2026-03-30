# Hush

A BepInEx mod for [On Together](https://store.steampowered.com/app/2688490/On_Together/) that lets hosts and clients filter chat messages. Matched words can be censored with asterisks or blocked entirely. The filter list persists between sessions and can be managed from in-game chat commands.

Hosts can also mute individual players permanently or for a fixed duration, suppressing their messages before they are relayed to any client.

When installed on the host, the filter and mute system are enforced server-side before the message is relayed, so every connected client receives the already-filtered version regardless of whether they have the mod. Clients with the mod installed also apply the word filter locally as a second layer.

## In-Game Commands

Type any command into the in-game chat. Commands start with `/` and are **not sent** to other players.

### Player muting

| Command | Short | Description |
|---|---|---|
| `/hushmute <player>` | `/hmu` | Permanently mute a player (host only) |
| `/hushtmute <player> <duration>` | `/htm` | Temporarily mute a player (host or delegate; e.g. `10m`, `1h30m`, `30s`) |
| `/hushunmute <player>` | `/humu` | Unmute a player (accepts Steam ID for offline players; host only) |
| `/hushgetmutes` | `/hgmu` | List all currently muted players with expiry (host only) |

**Player query formats** — name (partial match), Steam ID suffix (digits only), or `_host`.

### Mute delegates

The host can whitelist trusted non-host players to use `/hushtmute`. When a delegate issues the command, a hidden sentinel message is sent to the host; the host validates the whitelist and executes the mute server-side. The sentinel is never visible to other clients.

| Command | Short | Description |
|---|---|---|
| `/hushdelegateadd <player>` | `/hdda` | Add a player to the mute-delegate whitelist (host only) |
| `/hushdelegateremove <player>` | `/hddr` | Remove a player from the whitelist; accepts raw Steam ID for offline players (host only) |
| `/hushdelegatelist` | `/hddl` | List all current mute delegates (host only) |

The delegate list is saved alongside the mute list in `BepInEx/config/com.andrewlin.ontogether.hush.mutes.json`.

### Word management

| Command | Short | Description |
|---|---|---|
| `/hushaddword <word>` | `/haw` | Add a literal word to the filter |
| `/hushremoveword <word>` | `/hrw` | Remove a literal word from the filter |
| `/hushgetwords` | `/hgw` | List all filtered words |

### Pattern management

Patterns are raw regular expressions. Use inline flags to control matching behavior.

| Command | Short | Description |
|---|---|---|
| `/hushaddpattern <regex>` | `/hap` | Add a regex pattern to the filter |
| `/hushremovepattern <regex>` | `/hrp` | Remove a regex pattern from the filter |
| `/hushgetpatterns` | `/hgp` | List all filtered patterns |

**Pattern examples**

| Pattern | Behavior |
|---|---|
| `(?i)f+u+c+k` | Case-insensitive, matches repeated letters, no word boundary |
| `\bslur\b` | Exact whole-word match, case-sensitive |
| `(?i)\bslur\b` | Exact whole-word match, case-insensitive |

### Utility

| Command | Short | Description |
|---|---|---|
| `/hushtoggle` | `/ht` | Toggle the filter on or off |
| `/hushfilteraction <action>` | `/hfa` | Set the filter action: `Censor` or `Block` |
| `/hushcensorchar <char>` | `/hcc` | Set the character used to replace matched words in Censor mode |
| `/hushloadfilter` | `/hlf` | Reload the filter word list from disk |

## Configuration

Located in `BepInEx/config/com.andrewlin.ontogether.hush.cfg`

| Key | Default | Description |
|---|---|---|
| `General > EnableFeature` | `true` | Master switch for the mod |
| `General > ShowCommand` | `false` | Show typed commands in chat |
| `Filter > Action` | `Censor` | `Censor` replaces matches with asterisks. `Block` suppresses the entire message. |
| `Filter > CensorChar` | `*` | Character used to replace matched words when in Censor mode |
| `Filter > ConfigPath` | *(see below)* | Path to the filter word list JSON file |

The filter word and pattern list is stored separately in `BepInEx/config/com.andrewlin.ontogether.hush.filter.json` and is updated automatically whenever you add or remove an entry via a chat command.

The player mute list (permanent and timed) is stored in `BepInEx/config/com.andrewlin.ontogether.hush.mutes.json` and is updated automatically on every mute/unmute. Timed mute expiry times survive game restarts.

## Installation

Use r2modman or the Thunderstore app for the simplest install.

**Manual:**

| Step | Action |
|---|---|
| 1 | Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) into your On Together game folder |
| 2 | Copy `AndrewLin.Hush.dll` into `BepInEx/plugins/` |
| 3 | Launch the game -- config files will be generated automatically |

**Dependencies:** [BepInExPack](https://thunderstore.io/c/on-together/p/BepInEx/BepInExPack/), [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)

## Host-only vs. all-clients

Installing on the host is sufficient to enforce filtering for everyone in the session. Clients who also have the mod installed get an additional local filter pass that covers their own messages before they are sent.
