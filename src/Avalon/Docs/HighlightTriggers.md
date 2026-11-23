# Highlight Triggers
---

Highlight triggers color any text in the game terminal that matches a pattern, so the words you care about stand out.

A regular [trigger](Triggers.md) does something when a line arrives: it sends a command, runs a script or moves the line somewhere else. A highlight trigger doesn't do anything except change how matching text looks. Use one to make your character's name, a guildmate's name, a rare item or a dangerous monster jump out of a busy screen.

## Creating a Highlight Trigger

Highlight triggers are managed from the **Triggers** menu on the ribbon:

| Menu Item | Description |
|-----------|-------------|
| **New Highlight Trigger** | Creates a new highlight trigger. |
| **Edit Highlight Triggers** | Opens the Highlight Triggers Editor, where you can view, change or remove your highlight triggers. |
| **Reset Max Execution Statistics** | Resets the **Max Execution Time** of every trigger and highlight trigger (see [Performance](#performance)). |

A highlight trigger has the following settings:

| Setting | Description |
|---------|-------------|
| **Pattern** | The word to highlight, or a [regular expression](Triggers.md) to match. |
| **Foreground Color** | The color of the matching text. The default is dark red. |
| **Background Color** | The color behind the matching text. The default is black. |
| **Enabled** | Whether the highlight trigger is active. |
| **Lookup Key** | An optional name that scripts use to find and change the highlight trigger (see [Changing Highlights from Scripts](#changing-highlights-from-scripts)). |
| **Max Execution Time** | The longest time, in milliseconds, the pattern has taken to check a single line. |

Highlight triggers belong to a [profile](Profiles.md). They aren't included in [packages](Packages.md).

## How Highlighting Works

Highlight triggers work differently from regular triggers, and it's worth knowing how, because it affects both what they can do and how fast the client runs.

A regular trigger checks each line **once**, when it arrives from the game. A highlight trigger is applied **while the terminal is being drawn**. The game terminal is built on AvalonEdit, a text editor control, and every time it draws itself it asks each enabled highlight trigger to check every line that's currently visible and color whatever matches.

This has a few useful results:

- **Highlights apply to text that's already on screen.** Create a highlight trigger, or change its pattern or colors, and the text in the terminal updates right away. You don't need to wait for the game to send the line again.
- **Highlights don't change the text.** They only change how it's drawn, so copying text, logging and regular triggers all see the original line.
- **Turning highlights off is instant.** Disabling a highlight trigger removes its colors from everything on screen.

A few rules to keep in mind when writing patterns:

- **Only the first match on each line is highlighted.** If a line contains your target's name twice, only the first is colored.
- **The whole match is colored**, not just a captured group. The pattern `Bob says` colors both words. To color only `Bob` when he's speaking, use a lookahead: `Bob(?= says)`.
- **Patterns are case sensitive.** Start the pattern with `(?i)` to ignore case, for example `(?i)bob`.
- **When highlight triggers overlap**, the one lower in the list is drawn on top.
- Highlight triggers color the **game terminal** only, not [custom terminals](UI-Elements.md#new-terminal).
- Turning off **Triggers Enabled** for the profile (or the **Triggers** button on the status bar) turns off highlight triggers too.

## Performance

> **Important:** Because highlight triggers run every time the terminal is drawn, an inefficient regular expression can make the whole client slow. Write highlight patterns carefully.

The terminal is drawn far more often than lines arrive. It's redrawn when new text comes in, when you scroll, when you select text, and when the window is resized. Each redraw checks **every enabled highlight trigger against every visible line**. With 60 lines on screen and 20 highlight triggers, that's 1,200 pattern checks for a single redraw, and in a busy fight the terminal can redraw many times a second.

All of this happens on the same thread that runs the client's interface. A pattern that takes even a few milliseconds per line adds up quickly, and you'll notice it as:

- Scrolling that stutters or lags.
- Delays between typing and the text appearing in the command box.
- The client freezing briefly when a lot of text arrives at once.

Each pattern is given at most 250 milliseconds to check a line. A pattern that takes longer is stopped and an error is written to the [Log Window](UI-Elements.md#log-window), but by then the client has already paused, and it will pause again on the next redraw.

### Finding a Slow Highlight Trigger

Open **Edit Highlight Triggers** and look at the **Max Execution Time** column. It records the longest each pattern has taken to check a single line. A well written pattern should stay well below one millisecond. If one stands out, fix or disable it.

Use **Reset Max Execution Statistics** on the **Triggers** menu to clear the times after you've changed a pattern, so you can see whether it helped.

### Writing Fast Patterns

| Do | Avoid | Why |
|----|-------|-----|
| `Bob` | `.*Bob.*` | `.*` makes the pattern do much more work on every line, and it colors the whole line instead of just the word. |
| `\bBob\b` | `[A-Za-z]*Bob[A-Za-z]*` | `\b` matches whole words without scanning letter by letter. |
| `(Bob\|Sue\|Tim)` in one highlight trigger | Three highlight triggers for `Bob`, `Sue` and `Tim` | One pattern is checked once per line instead of three times. |
| `^You are hungry\.` | `You are hungry.` | `^` lets the pattern give up straight away on lines that don't start the right way. Escape `.` when you mean a literal period. |
| `\d+ gold` | `(\d+)+ gold` | Nested repeats like `(a+)+` can take an extremely long time on lines that almost match. This is the most common cause of a pattern hitting the 250 millisecond limit. |

A few other tips:

- **Keep the list short.** Disable highlight triggers you don't need right now rather than leaving them all on.
- **Use the Regex Builder.** Right click text in a terminal and choose **Open in Regex Builder** to test a pattern against real lines before you save it.
- **If you need to react to a line, use a regular trigger.** A regular trigger runs once per line, so it's the better choice for anything other than coloring text.

## Changing Highlights from Scripts

Scripts can create and update highlight triggers by their **Lookup Key**. This is useful for highlights that change as you play, such as your current target or the members of your group.

| Function | Description |
|----------|-------------|
| `terminal.Highlight(key, text)` | Sets the pattern of the highlight trigger with this lookup key to `text`, enables it, and colors it red on black. Creates the highlight trigger if it doesn't exist. |
| `terminal.Highlight(key, text, foreground, background)` | The same, with your own colors, such as `"#FFFF00"`. |
| `terminal.HighlightAppend(key, text)` | Adds `text` to the highlight trigger's list of words, so the pattern `(Bob)` becomes `(Bob\|Sue)`. Creates the highlight trigger if it doesn't exist. |
| `terminal.HighlightRemove(key, text)` | Removes `text` from the highlight trigger's list of words. |
| `terminal.HighlightEnable(key)` | Enables the highlight trigger. |
| `terminal.HighlightDisable(key)` | Disables the highlight trigger. |

For example, an [alias](Aliases.md) `target` that runs JavaScript could set your target and highlight it at the same time:

```javascript
terminal.SetVariable("Target", "%1");
terminal.Highlight("target", "%1", "#FFFF00", "#800000");
```

And a trigger that matches someone joining your group could add them to a group highlight:

```javascript
terminal.HighlightAppend("group", "%1");
```

The text passed to these functions becomes part of a regular expression, so the [performance](#performance) advice above applies to it as well.

[Return to Table of Contents](Index.md)
