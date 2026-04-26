# JaideAdapter

Bridges Alpha's command system into Jaide's CommandAPI so that [CommandTypeahead](https://thunderstore.io/c/on-together/p/jaide/CommandTypeahead/) shows autocomplete for all Alpha-based mod commands.

## What it does

On startup, reads every command registered with Alpha's `ChatCommandManager` and mirrors them into Jaide's `CommandRegistry`. The actual command handling stays with Alpha - this mod only provides the metadata (name, description, category) that CommandTypeahead needs for its dropdown.

## Requirements

- [Alpha](https://thunderstore.io/c/on-together/p/AndrewLin/Alpha/)
- [CommandAPI](https://thunderstore.io/c/on-together/p/jaide/CommandAPI/)
- [CommandTypeahead](https://thunderstore.io/c/on-together/p/jaide/CommandTypeahead/) 

## Installation

Install via r2modman or Thunderstore mod manager.
