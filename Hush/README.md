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
| `/hushtmute <player> <duration>` | `/htm` | Temporarily mute a player (host, delegate, or mod admin; e.g. `10m`, `1h30m`, `30s`) |
| `/hushunmute <player>` | `/humu` | Unmute a player (host, delegate, or mod admin; accepts Steam ID for offline players) |
| `/hushgetmutes` | `/hgm` | List all currently muted players with expiry (host only) |

**Player query formats** - name (partial match), Steam ID suffix (digits only), or `_host`.

### Player banning

Ban commands write to the game's own ban list (`DataManager.BanData`), so bans persist and are enforced natively on every session start.

| Command | Short | Description |
|---|---|---|
| `/hushban <player>` | `/hb` | Ban an online player (host or delegate) |
| `/hushbanoffline <steamid> <nick>` | `/hbo` | Ban a player by Steam ID without them being online (host only) |
| `/hushunban <player\|steamid>` | `/hub` | Remove a ban; accepts Steam ID for offline removal (host only) |
| `/hushgetbans` | `/hgb` | List all banned players (host only) |

### Mute & ban delegates

The host can whitelist trusted non-host players to use `/hushtmute`, `/hushunmute`, and `/hushban`. When a delegate issues one of these commands, a hidden sentinel message is sent to the host; the host validates the whitelist and executes the action server-side. The sentinel is never visible to other clients. Delegates also add the ban to their own local ban list.

**Delegates do not need Hush installed.** Any player can manually type a sentinel directly into chat and the host will execute it if the sender is on the whitelist:

| Sentinel format | Action |
|---|---|
| `hush:tmute:<player_or_steamid>:<seconds>` | Timed mute |
| `hush:unmute:<player_or_steamid>` | Unmute |
| `hush:ban:<player_or_steamid>` | Ban |

The `<player_or_steamid>` field accepts either a Steam ID64 (17-digit number) or any player query string (partial name, name suffix, Steam persona name). Steam IDs are resolved directly; name queries resolve to the first online match.

**Mod admins** - Steam IDs listed in `Alpha.ModAdmins` bypass the delegate whitelist and can relay all commands regardless of whitelist status.

| Command | Short | Description |
|---|---|---|
| `/hushdelegateadd <player>` | `/hda` | Add a player to the delegate whitelist (host only) |
| `/hushdelegateremove <player>` | `/hdr` | Remove a player from the whitelist; accepts raw Steam ID for offline players (host only) |
| `/hushdelegatelist` | `/hdl` | List all current delegates (host only) |

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
