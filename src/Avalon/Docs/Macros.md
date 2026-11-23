# Macros
---

Macros bind a command to a key or key combination so a single key press sends it to the game.

A macro is the simplest way to put your most used commands at your fingertips. Instead of typing `north` or `quaff heal` over and over, you press a key and the client sends the command for you, exactly as if you had typed it.

## Why Use Macros?

- **Movement:** Walk with the number pad instead of typing directions. This is the most common use of macros.
- **Combat:** Put your attacks, heals and escape commands on function keys so you can react quickly.
- **Utility:** Open client windows, toggle trigger groups or run scripts with a single key.

## A Simple Example

| Field | Value |
|-------|-------|
| Key Combination | `F1` |
| Command | `cast 'cure light' self` |

Now, pressing `F1` while you're in the game terminal casts the spell.

## Creating a Macro

Macros are managed from the **Macros** menu on the ribbon:

| Menu Item | Description |
|-----------|-------------|
| **New Macro** | Creates a new macro. |
| **Edit Macros** | Opens the Macros Editor where you can view, change or remove your existing macros. |

To set the **Key Combination**, click in the key combination box and press the keys you want to use. Don't type the name of the key. The client records the key press for you, including any modifier keys that are held down, so holding `Ctrl` and `Shift` and pressing `F1` records `Ctrl+Shift+F1`.

| Setting | Description |
|---------|-------------|
| **Key Combination** | The key, optionally combined with `Ctrl`, `Alt` and `Shift`, that runs the macro. |
| **Command** | The command to send when the key is pressed. |

Key names use the Windows names for keys, for example `F5`, `NumPad8`, `Ctrl+Alt+Q` or `Oem3` (the `` ` `` / `~` key).

## What a Macro Can Send

A macro's command is processed the same way as text typed into the command box, so it can do anything you can do by typing:

| Example | What It Does |
|---------|--------------|
| `kick` | Sends a single command to the game. |
| `get all corpse;sacrifice corpse` | Sends multiple commands, separated with a semicolon. |
| `heal` | Runs an [alias](Aliases.md) named `heal`, including aliases that run JavaScript. |
| `#group-enable combat` | Runs a hash command, in this case turning on a group of [triggers](Triggers.md). |
| `+3n 2e (open door) n` | Speedwalks a set of [directions](Directions.md). |
| `#dir` | Opens the quick directions window. |

Macros don't have a JavaScript option of their own. To run a script from a key press, create an alias with **Execute As** set to `JavaScript` and point the macro at the alias.

Commands sent by a macro are shown in the terminal but aren't added to your command history, so pressing the up arrow won't bring them back.

## Number Pad Movement

When the Setup Wizard creates a profile that doesn't have any macros yet, it adds a set of movement macros for you:

| Key | Command |
|-----|---------|
| `NumPad7` | `nw` |
| `NumPad8` | `n` |
| `NumPad9` | `ne` |
| `NumPad4` | `w` |
| `NumPad5` | `scan` |
| `NumPad6` | `e` |
| `NumPad1` | `sw` |
| `NumPad2` | `s` |
| `NumPad3` | `se` |
| `F5` | `#dir` |

Number pad keys only report as `NumPad` keys while **Num Lock** is on. If your movement keys stop working, check Num Lock first.

## Things to Know

- **Macros only work in the game terminal.** They fire when a game terminal is the active window, so pressing the key while you're typing in an editor or another window won't send anything to the game.
- **A macro takes over the key.** When a key combination matches a macro, the key press is used by the macro and doesn't reach the command box. A macro on a plain letter like `A` means you'll no longer be able to type that letter. Use function keys, number pad keys or combinations with `Ctrl` or `Alt` instead.
- **Macros take precedence over the client's shortcuts.** If you assign a macro to a combination the client already uses, such as `Ctrl+D` for the directions window, the macro wins. See [Keyboard Shortcuts](KeyboardShortcuts.md) for the built in shortcuts.
- **Macros belong to the profile.** Each [profile](Profiles.md) has its own set of macros. Unlike aliases and triggers, macros aren't character specific and aren't included in [packages](Packages.md).

[Return to Table of Contents](Index.md)
