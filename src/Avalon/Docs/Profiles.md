# Profiles
---

Profiles save your personalized settings and configurations to quickly switch between different gaming setups.

## What is a Profile?

A profile represents one game you play. It holds everything Avalon needs to connect to that game and everything you've built to make playing it easier. If you play more than one game, or want to keep very different setups for the same game, you create a profile for each.

A profile contains:

| Area | What's Stored |
|------|---------------|
| **Connection** | The profile's name, plus the hostname (or IP address) and port of the game server. |
| **Game Objects** | Your [aliases](Aliases.md), [triggers](Triggers.md), highlight triggers, [macros](Macros.md), [directions](Directions.md), [variables](Variables.md) and [scripts](Scripting.md). |
| **Packages** | Information about the [packages](Packages.md) that have been installed into the profile. |
| **Events** | Commands or scripts that run when you connect (**OnConnectCommand**), when the profile loads (**OnProfileLoadedEvent**), when the profile closes (**OnProfileClosedEvent**) and when a game tick passes (**OnTickEvent**). |
| **Terminal Options** | Switches that turn triggers, replacement triggers and aliases on or off for the whole profile, auto-complete settings, whether GMCP is enabled, whether your last command is saved to the `LastCommand` variable, and the game's tick length. |
| **Appearance** | The color of the profile's tab, the colors of the command box, block caret, and the visibility of the stats bar. |

Because your game objects belong to a profile, the triggers and aliases you create for one game won't interfere with another.

## Profiles and Characters

A profile is tied to a game, not a character. If you play several characters on the same game, they can all share one profile. Aliases and triggers have a **Character** field that lets you limit them to a specific character, so you can keep everything in one place and still have character specific behavior. Leaving the Character field blank makes the alias or trigger available to every character.

## Working with Profiles

Profiles are managed from the **Profile** menu on the ribbon:

| Menu Item | Description |
|-----------|-------------|
| **New Profile** | Creates a new profile. At a minimum, give it a name along with the hostname and port of the game. |
| **Edit Profiles** | Opens the Profiles Editor where you can change or remove your existing profiles. |
| **Open Profile** | Choose a profile, open it in a new terminal tab and connect to the game. |
| **Import Profile from File** | Adds a profile from a `.profile` file to your profile list. |
| **Export Profile to File** | Saves a single profile to a `.profile` file. |
| **Backup Settings** | Makes a permanent backup of all of your settings (see below). |
| **Restore from Backup** | Retrieves one of your saved backups (see below). |

Multiple profiles can be open at the same time, each in its own tab.

### Startup Options

Two client settings control what happens with profiles when Avalon starts:

- **Load Last Profile On Startup**: Automatically opens and connects to the profile you used last. This is on by default.
- **Show Open Profile Dialog On Startup**: Shows the profile selection dialog when the client starts.

### Sharing a Profile

Exporting a profile is the easiest way to move a single game setup to another computer, keep a copy before making large changes, or share your setup with a friend. Importing a profile adds it to your profile list as an additional entry; it does not replace any of your existing profiles.

> **Tip:** Exported profiles contain everything in the profile, including all of your variables. Review your variables before sharing a profile with someone else in case they hold anything you'd rather keep private.

## Where Profiles are Saved

All of your profiles are saved together with the rest of the client's settings in a single file named `AppSettings.json`. By default this file lives in:

```
Documents\ApexGate\Avalon\AppSettings.json
```

Settings are written when the client closes. To protect against a crash or power loss in the middle of a save corrupting your settings, the client first writes everything to `AppSettings.json.bak` and only then copies it over `AppSettings.json`. If something goes wrong partway through, your previous `AppSettings.json` is left intact.

## Backups

Your profiles can represent hours of work, so Avalon keeps backups of your settings inside the client's database. Each backup is a complete snapshot of `AppSettings.json`, which means it includes **all** of your profiles and the client's settings, not just a single profile. Backups are compressed before they're stored and record the date they were made and the version of the client that made them.

There are two kinds of backups:

### Automatic Backups

Every time Avalon starts, it automatically backs up your settings in the background. These automatic (temporary) backups are cleaned up over time so they don't grow without limit:

- Automatic backups older than **Days Of Backups To Keep** are deleted. The default is `14` days and the maximum is `30`.
- If the total number of backups exceeds **Backup Limit**, the oldest automatic backups are deleted until the total is back under the limit. The default is `25`.

Both of these settings can be found in the **Database** category of the client settings.

### Permanent Backups

Selecting **Backup Settings** from the **Profile** menu creates a permanent backup. Permanent backups are never deleted by the automatic cleanup, which makes them ideal for saving a known good setup before you make major changes, install a new package or update the client.

> **Note:** Permanent backups count toward the **Backup Limit** even though they're never removed automatically. If you make a large number of permanent backups you'll have fewer automatic backups to fall back on.

### Restoring a Backup

Selecting **Restore from Backup** from the **Profile** menu shows a list of every backup along with its date, the client version that created it and whether it is a temporary backup. Choose a backup and you'll be prompted for a location to save it as a JSON file.

The backup is saved to a file rather than applied immediately so you can decide how to use it:

- **To restore everything:** Close Avalon, rename your current `AppSettings.json` (so you can go back to it if needed), copy the backup file into its place and name it `AppSettings.json`, then start Avalon again.
- **To recover something specific:** Open the backup file in a text editor and copy out the alias, trigger, script or other item you need.

> **Important:** Always close Avalon before replacing `AppSettings.json`. The client saves its settings when it exits, so replacing the file while the client is running will result in your restored file being overwritten.

[Return to Table of Contents](Index.md)
