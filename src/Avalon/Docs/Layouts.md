# Layouts
---

Layouts are the arrangement of windows and panels in the client. Avalon uses a docking system, so you can drag every panel to wherever you want it and build the screen that suits the way you play.

## The Parts of the Screen

The client's main window is made up of two kinds of windows:

- **Game terminals** live in the center workspace. Each [profile](Profiles.md) you open gets its own terminal tab.
- **Tool windows** surround the workspace and show supporting information, like your variables, affects or chat.

The following tool windows are available. Any that are closed can be reopened from the **Layout** menu on the ribbon or from the Tool Window Manager.

| Tool Window | Description |
|-------------|-------------|
| **Communication** | A separate terminal for chat. The Channels package moves channel messages here so they don't scroll away in the middle of combat. |
| **Variables** | Shows the [variables](Variables.md) you've chosen to display and keeps them up to date as they change. |
| **Affects** | Shows the spells and effects currently on your character. |
| **Alias Button Actions** | Buttons that run your [aliases](Aliases.md) with a click. |
| **Movement** | Clickable movement buttons. |
| **Properties** | Shows the settings of the active profile or of the item you're editing. |
| **Log Window** | Messages from the client, such as errors and the results of package installs. |
| **Database** | Work with the client's [database](Database.md). |
| **Notes** | A place to keep notes while you play. |
| **Tool Window Manager** | Lists every tool window and lets you open, close, dock or float it (see below). |
| **Custom terminals** | Additional terminals you create yourself (see below). |
| **Vitals panels** | Named resource bars that scripts can update. |

## The Docking System

Every window in the client can be moved, resized, stacked, torn off into its own window and snapped back in. There's no fixed set of layouts to choose from; you build the layout you want by dragging things where you'd like them.

### Resizing

Drag the divider between any two windows to make one bigger and the other smaller.

### Tearing a Window Off

Click and hold a window's title bar (or its tab, if it shares a space with other windows), then drag it away. The window tears off and becomes a **floating window** that you can place anywhere, including on a second monitor. Floating windows stay on top of the client, which is handy for keeping chat or your variables in view while the main window is full screen on another monitor.

Game terminal tabs can be torn off as well, so you can put two games side by side or move a terminal to another monitor.

### Snapping a Window Into Place

While you're dragging a window, **dock guides** appear: small arrow targets in the middle of the area you're hovering over and at the edges of the main window.

- Drop the window on an **arrow** in the center guide to dock it to that side of the area under your mouse, splitting the space between them.
- Drop it on the **center** of the guide to add it as a tab alongside the windows already there.
- Drop it on one of the **guides at the edge of the main window** to dock it along that entire edge.

A highlighted preview shows where the window will land before you let go. If you drop a window anywhere other than a guide, it stays floating.

To put a floating window back, drag it by its title bar onto a dock guide again. Double clicking a floating window's title bar also returns it to where it was last docked.

### Tabs

Windows dropped on the center of a guide share the same space as tabs. Click a tab to switch between them, or drag a tab out to separate it again. This is a good way to keep windows you use only occasionally, like Properties or the Log Window, close at hand without them taking up room.

### Auto Hide

Click the **pin** icon on a docked window's title bar to auto hide it. The window collapses into a tab along the edge of the client and slides out when you click the tab, then tucks itself away again when you click elsewhere. Click the pin again while it's open to dock it normally. Auto hide is useful for windows you check now and then without giving up terminal space.

### Window Menus

Right click a window's title bar or tab for options such as floating, docking, auto hiding or closing it. For game terminal tabs this menu also includes:

- **Save Log**: Saves the text of the terminal to a file, with or without color codes.
- **Profile Properties**: Shows the profile's settings in the Properties window.

### The Tool Window Manager

The **Tool Window Manager** lists every tool window along with whether it's open, floating, docked or auto hidden. Select a window and use the toolbar to **Activate**, **Open**, **Close**, **Dock**, **Move to MDI** (turn it into a tab in the center workspace), **Auto-Hide** or **Float** it.

If you lose track of a window, for example a floating window that was left on a monitor that's no longer connected, the Tool Window Manager is the easiest way to bring it back. Open it from **Show Tool Window Manager** on the **Layout** menu.

## Custom Terminals

You can add as many extra terminals as you like and route text to them. They're useful for separating out things you don't want to lose in the main game window: tells, group chat, loot logs, or the output of a script.

Choose **Add New Terminal** from the **Layout** menu and give it a name. The new terminal appears as a tool window that can be docked, floated and tabbed like any other.

Text can be sent to a custom terminal from a [trigger](Triggers.md), [alias](Aliases.md) or script. The terminal's name is the name you gave it with any spaces removed.

```
#echo -t Loot You picked up %1.
```

```javascript
terminal.WriteLine("Loot", "You picked up %1.");
```

When a script writes to a terminal that doesn't exist yet, the client creates it for you. `#echo` only writes to terminals that already exist. See [Scripting](Scripting.md) for the other terminal functions.

## Vitals Panels

Choose **Layout → Add Vitals Panel** and give it a name, such as `Character` or `Party Tank`. Enter an existing name to reopen that panel. Panels can be docked, floated, or tabbed like other tool windows.

Each new panel starts with HP, Mana, and Move. Right-click inside the panel and choose **Configure Vitals** to add or remove labels for your game. Avalon saves these labels with your settings. Current readings come from your game's triggers or scripts and start empty on launch.

```javascript
ui.SetVital("Character", "HP", 75, 100);
ui.SetVital("Character", "Mana", 42, 100);
ui.SetVital("Character", "Move", 18, 100);
```

Bars are green at 60% or higher, yellow from 30% up to 60%, and red below 30%. See [Scripting](Scripting.md#ui) for the panel methods.

## Layout Menu

The **Layout** menu on the ribbon has the following options:

| Menu Item | Description |
|-----------|-------------|
| **Reset Layout** | Puts all of the tool windows back in their default positions. Use this if your layout gets into a state you don't want to untangle by hand. |
| **View Current Layout** | Opens a text view of the current layout. This is mostly useful for troubleshooting. |
| **Show Tool Window Manager** | Opens the Tool Window Manager. |
| **Show *window name*** | Opens the named tool window. |
| **Add New Terminal** | Creates a new custom terminal. |
| **Add Vitals Panel** | Opens or creates a named vitals panel. |

## How Layouts are Saved

Your layout is saved automatically when the client closes and restored the next time it starts. This includes dock positions, pane sizes, tab groups, auto-hidden tools, and floating windows. Choose **Exit (without saving Settings)** to keep the layout from your previous session.

A few things to know about how layouts are stored:

- **The layout belongs to the client, not to a profile.** The same arrangement is used whichever profile you open.
- **Layouts are saved per computer.** If your settings are shared between computers, each one keeps its own layout, so a layout built for a three monitor desktop won't be forced onto a laptop.
- **Game and custom terminals reopen automatically.** Game terminals reopen disconnected. Use **Connect** to resume playing.

The layout is stored in `DockLayout.json`. Choose **Open Settings Folder** to find it.

[Return to Table of Contents](Index.md)
