# Packages
---

Packages bundle aliases, triggers, directions, variables and scripts into a single unit that can be installed, updated, shared and removed as one.

## What is a Package?

Building a good setup for a game takes time. You might write a dozen triggers to capture your stats, a few scripts to process them, and some aliases to show them. A package groups all of those pieces together under one name so they can be handled as a set.

A package can contain:

| Item | Description |
|------|-------------|
| **[Aliases](Aliases.md)** | Command shortcuts. |
| **[Triggers](Triggers.md)** | Automatic responses to text from the game. |
| **[Directions](Directions.md)** | Saved paths between rooms. |
| **[Variables](Variables.md)** | Values used by the other items, such as settings with sensible defaults. |
| **[Scripts](Scripting.md)** | JavaScript functions used by the other items. |

[Macros](Macros.md) and highlight triggers aren't included in packages.

Every item in a profile has a **Package** setting. Items that belong to a package have it set to that package, which is how the client knows what to include when the package is built and what to remove when it's uninstalled.

Packages are installed into a [profile](Profiles.md), so a package installed for one game won't affect another.

## Why Use Packages?

- **Get started quickly:** Install what other players have already built instead of writing it yourself.
- **Try things out safely:** Uninstalling a package removes every piece that came with it, so you're not left hunting down stray triggers.
- **Stay up to date:** When an author publishes a new version of a package, the Package Manager tells you an update is available.
- **Share your work:** Turn your own setup into a package and give it to your friends or publish it for everyone who plays your game.

## The Package Manager

The Package Manager is the easiest way to find packages. Open it from **Package Manager** on the **Packages** menu on the ribbon.

The Package Manager shows the packages that have been submitted for the game your active profile connects to, read live from the package repository. Many pre-built packages are available, covering things like:

- Processing GMCP data from the game into variables and the stats bar.
- Moving channels into the Communication window.
- Tracking your affects.
- Gagging spammy combat messages.
- Utilities like stat rollers.

Each package is shown as a card with its name, description, author, version and download count. Badges on the card show when a package is **Installed**, when an **Update Available**, or when it **Requires** a newer version of Avalon than the one you're running. Packages that need a newer client are greyed out until you update.

Use the search box at the top of the list to filter packages, and the refresh button in the window's title bar to reload the list.

Click a card to see the package's full documentation on the right, then:

- **Install** adds the package to your active profile. If it's already installed, installing it again updates it to the latest version.
- **Uninstall** removes the package and everything that came with it.

You must have a game terminal open and active to install a package, since packages are installed into the active profile.

### Starting From a Pre-Built Package

Pre-built packages are a great starting point for your own setup. Install one that's close to what you want, look at how its triggers, aliases and scripts work in the game editors on the ribbon, then adapt them.

Keep in mind that items installed by a package still belong to that package. If you change them and later update or reinstall the package, **your changes will be replaced** with the package's version. To make an item your own, clear its **Package** setting, or copy it and change the copy.

### Private Packages

Packages can be published as private. Private packages don't appear in the Package Manager's list, but anyone with the package's unique ID can install them. Click the download button in the Package Manager's title bar and enter the ID to install a private package. This is a good way to share a package with a guild or a small group of friends without listing it publicly.

## When You First Start

The first time Avalon runs, it installs a few packages into the starter profile for you:

| Package | Description |
|---------|-------------|
| **GMCP** | Processes the character and room information the game sends over [GMCP](GMCP.md). This keeps variables like `Room` up to date. |
| **Channels** | Moves channel messages into the Communication window. |
| **Affects** | Scripts and aliases for tracking your affects. |
| **Common Variables** | A set of commonly used variables. |

These are regular packages and can be updated or uninstalled like any other.

## Making Your Own Package

### 1. Create the Package

Choose **New Package** from the **Packages** menu and fill in its details:

| Setting | Description |
|---------|-------------|
| **Package Name** | The name of the package. Required. |
| **Author** | Your name. Required. |
| **Version** | The version number of the package, starting at `1`. |
| **Description** | A short description shown on the package's card. |
| **Markdown Documentation** | Full documentation, written in Markdown, shown when someone selects your package in the Package Manager. Explain what the package does, how to use it and any variables people may want to change. |
| **On Install Event** | JavaScript that runs when the package is installed. Use it to welcome the user, print instructions or set things up. |
| **Minimum Client Version** | The oldest version of Avalon the package works with, if it relies on newer client features. |
| **Site Address** | The address of the game the package is for. |
| **Is Private** | Whether the package should be private when it's published. |

### 2. Add Items to the Package

Open each alias, trigger, direction, variable and script you want to include and set its **Package** setting to your new package. Everything with that package set is included when the package is built.

A few tips for a package other people will install:

- Give your items clear names and put related triggers in a [group](Triggers.md#groups) so they can be turned on and off together.
- Leave the **Character** setting blank so your triggers and aliases work for everyone.
- Don't include variables that hold anything personal.
- Variables in a package replace any variable with the same name when it's installed, so use names that are unlikely to collide with the user's own.

### 3. Build the Package File

Choose **Compile Packages** from the **Packages** menu. The client builds a `.package` file for every package in the active profile and opens the folder they were saved to:

```
Documents\ApexGate\Avalon\Packages
```

Statistics such as how many times each trigger and alias has fired are reset in the package file, so your personal usage isn't shared.

### 4. Share the Package

A `.package` file can be shared like any other file. To install one, choose **Import Package from File** from the **Packages** menu and select it. You can select more than one file at a time.

Package authors with an ApexGate account who are signed in can also publish packages to the repository. Open **Edit Packages**, right click a package and choose **Publish** (or **Unpublish** to remove it). Once published and approved, a public package appears in the Package Manager for its game. A private package can be installed by anyone who has its ID. **Copy Package Unique ID** copies the ID you'll need to give out for a private package.

### 5. Release Updates

When you make changes, increase the package's **Version** before you publish again. Players who have an older version installed will see **Update Available** in the Package Manager.

## How Installing Works

When a package is installed, from the Package Manager or from a file:

1. Any version of the package that's already installed is uninstalled first, removing every item that belongs to it.
2. The package's aliases, triggers, directions, variables and scripts are added to the profile. If the profile already has an item with the same unique ID, or a variable with the same name, it's replaced.
3. The package's **On Install Event** runs, if it has one.
4. The package is added to the profile's list of installed packages, which you can see in **Edit Packages**.

To uninstall a package, select it in the Package Manager and choose **Uninstall**, or right click it in **Edit Packages** and choose **Uninstall**. Every item that belongs to the package is removed from the profile, including any items you added to the package yourself.

> **Tip:** Make a permanent backup with **Backup Settings** on the **Profile** menu before trying out a large package. See [Profiles](Profiles.md#backups).

[Return to Table of Contents](Index.md)
