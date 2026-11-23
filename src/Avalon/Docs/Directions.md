# Directions
---

Directions are saved paths from one place in the game to another that the client can walk for you.

Most games are made up of rooms connected by exits, and getting anywhere means typing a long string of movement commands. Directions let you record that path once, give it a name, and from then on travel there with a single command, a key press, or a quick pick from a list.

## Speedwalking

Directions are built on **speedwalks**, a short way of writing a path. A speedwalk is a list of steps separated by spaces:

| Step | Meaning |
|------|---------|
| `n` `s` `e` `w` `ne` `nw` `se` `sw` `u` `d` | A single move in that direction. |
| `3n` | A number in front of a step repeats it, so `3n` is the same as `n n n`. |
| `(open door)` | Anything in parentheses is sent as a single command, which lets you include commands with spaces in the middle of a path. |
| `enter` | Any other single word is sent as is. |

For example, `3n 2e (open door) n u` sends `n`, `n`, `n`, `e`, `e`, `open door`, `n`, `u`.

You don't need a saved direction to speedwalk. Start a command with `+` (the **Speedwalk Character**, which can be changed in the client settings) or use the `#walk` command:

```
+3n 2e (open door) n u
#walk 3n 2e (open door) n u
```

> **Tip:** Any number in a step is treated as a repeat count, so avoid commands that contain numbers inside a speedwalk. `(say 2 more rooms)` won't do what you expect.

## Creating Directions

Directions are managed from the **Directions** menu on the ribbon:

| Menu Item | Description |
|-----------|-------------|
| **New Direction** | Creates a new direction. |
| **Edit Directions** | Opens the Directions Editor where you can view, change or remove your directions. |
| **Show Available Directions from Room** | Opens the quick directions window (see below). |
| **Start Recording** / **Stop Recording** | Records your movement so the client can write the speedwalk for you. |

A direction has the following settings:

| Setting | Description |
|---------|-------------|
| **Name** | The name of the destination. This is what you'll type with `#go` and search for in the directions window, so name it after where it goes, for example `Temple` or `Bank`. |
| **Speedwalk Directions** | The path to walk, in speedwalk format. |
| **Starting Room Name** | The name of the room the path starts from. |
| **Ending Room Name** | The name of the room the path ends in. |
| **Package** | The [package](Packages.md) this direction belongs to, if any. |

The starting and ending room names should be the room names exactly as the game shows them. The client uses them to work out which directions are available from where you're standing and to chain directions together.

### Recording a Path

Rather than writing the speedwalk by hand, you can let the client record it:

1. Stand in the room you want to start from.
2. Choose **Start Recording** from the **Directions** menu.
3. Walk to your destination as you normally would. Any other commands you need along the way, like `open door`, are recorded too.
4. Choose **Stop Recording**.

The client prints the starting room, the speedwalk and the ending room to the terminal. Copy them into a new direction. Long directions like `north` are shortened to `n`, and other commands are wrapped in parentheses for you.

### Creating the Return Trip

In the Directions Editor, right click a direction and choose **Create Reverse Direction**. The client creates a new direction that walks the path backwards from the ending room to the starting room and names it after the original starting room. Commands in parentheses are kept, but they aren't reversed, so check the result for things like `(open door)` that might need to change for the trip back.

The `#reverse-path` command does the same thing for any speedwalk and prints the result to the terminal:

```
#reverse-path 3n 2e u
```

## Knowing Where You Are

Using directions by name depends on the client knowing the room you're in. That's tracked in the `Room` [variable](Variables.md). The GMCP package that's installed when you first run Avalon keeps `Room` up to date on games that send room information through [GMCP](GMCP.md). On other games you can set it yourself with a trigger that matches the room name line, for example with a named capture `(?<Room>...)`.

## Traveling

### The Quick Directions Window

Press `Ctrl+D` (or use `#dir`, or **Show Available Directions from Room** on the ribbon) to open the quick directions window. It lists every direction that starts in your current room. Start typing to filter the list, then press `Enter` or click a destination to walk there. The window closes on its own when it loses focus.

The window also chains directions together. If you have a path from the Temple to the Market and another from the Market to the Docks, standing in the Temple will offer you the Docks as well, and walking it combines both paths. Paths can be chained up to five directions deep. This is where accurate starting and ending room names pay off: a handful of short paths between common hubs gives you routes to many more places.

### The #go Command

`#go <name>` walks to the direction with that name:

```
#go Bank
```

- If a direction with that name starts in your current room, it's walked.
- If not, the client looks for a direction from your current room to that direction's starting room. If one exists, both are walked back to back.
- Otherwise the client tells you that no path was found from your current room.

`#go` is handy in [aliases](Aliases.md) and [macros](Macros.md), for example a function key that always takes you to the bank.

[Return to Table of Contents](Index.md)
