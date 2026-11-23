# Triggers
---

Triggers are automated responses that execute commands when specific text appears.

Every line of text the game sends to you is checked against your list of enabled triggers. When a line matches a trigger's **pattern**, the trigger fires and runs its **command**, either by sending text to the game exactly as if you had typed it, or by running a JavaScript snippet.

## Why Use Triggers?

Triggers let the client react to the game for you. Common uses include:

- **Convenience:** Automatically loot and sacrifice a corpse when you kill something.
- **Survival:** Quaff a healing potion or flee when the game reports you're badly hurt.
- **Awareness:** Play a sound, flash the window, or pop up a toast notification when someone sends you a tell.
- **Tracking:** Capture information from the game (your gold, the name of the person who just attacked you, the room you're in) into [Variables](Variables.md) for use by aliases, other triggers and scripts.
- **Noise reduction:** Hide (gag) spammy lines you don't want to see, or rewrite lines so they're easier to read.

> **A note on fair play:** Many games have rules about automation, often called "botting". Triggers that play the game while you're away from the keyboard may be against the rules. Check your game's policies before automating anything significant.

## A Simple Example

Suppose that whenever you kill a monster the game prints:

```
The goblin is DEAD!!
```

You could create a trigger with:

| Field | Value |
|-------|-------|
| Name | Auto Loot |
| Pattern | `is DEAD!!$` |
| Command | `get all corpse` |

Now, every time something dies, the client sends `get all corpse` to the game for you.

## Patterns

A trigger's pattern is a [regular expression](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) (regex). If you're new to regular expressions, the most useful pieces are:

| Pattern | Meaning |
|---------|---------|
| `^` | The start of the line. |
| `$` | The end of the line. |
| `.` | Any single character. |
| `.*` | Any amount of anything (including nothing). |
| `\d+` | One or more digits. |
| `\w+` | One word (letters, digits or underscore). |
| `(...)` | A capture group. Whatever matches inside the parentheses is captured so you can use it in the command. |
| `\.` `\(` `\[` | Characters that have special meaning in a regex must be escaped with a `\` to be matched literally. |

A few tips:

- Patterns are **case sensitive**. `goblin` will not match `Goblin`. Add `(?i)` to the start of the pattern to make it case insensitive.
- Use `^` and `$` to anchor your pattern so it doesn't match something unexpected. For example, a pattern of `tells you` would match any line containing those words, including a line someone crafted to fool your trigger. `^(\w+) tells you '(.*)'$` is much safer.
- Patterns are matched against the text of the line with color codes removed.
- If a pattern isn't a valid regular expression, the client will echo an error in the terminal and the pattern won't be saved.
- The **Regex Builder** tool can help you build and test a pattern before you use it.

## Using Captured Text in the Command

Text captured by the pattern can be placed into the command with `%` placeholders:

| Placeholder | Value |
|-------------|-------|
| `%0` | The entire line that fired the trigger. |
| `%1`, `%2`, `%3` ... | The text captured by the first, second, third ... capture group. |

**Example: Thank people who give you things.**

| Field | Value |
|-------|-------|
| Pattern | `^(\w+) gives you (.*)\.$` |
| Command | `say Thank you for %2, %1!` |

When the game sends `Gandalf gives you a loaf of bread.`, the client sends `say Thank you for a loaf of bread, Gandalf!`.

### Named Captures Become Variables

A regular trigger saves each non-blank named capture, such as `(?<name>...)`, into a [variable](Variables.md) with the same name. This works even when the trigger has no command or gags the matching line. An optional capture that doesn't match leaves the variable's current value in place.

**Example: Remember who last attacked you.**

| Field | Value |
|-------|-------|
| Pattern | `^(?<Attacker>\w+) attacks you!$` |
| Command | `#echo Attacked by @Attacker` |

After this fires, the `Attacker` variable holds the attacker's name and can be used in aliases or other triggers as `@Attacker`.

## Trigger Settings

| Setting | Description |
|---------|-------------|
| **Name** | A friendly, descriptive name for the trigger. Not required, but it makes your trigger list much easier to manage. |
| **Pattern** | The regular expression that is matched against each line from the game. |
| **Command** | The command to send to the game, or the JavaScript to run, when the pattern matches. |
| **Execute As** | `Command` sends the command to the game as if you typed it (aliases and hash commands work here too). `JavaScript` runs the command as a script. |
| **Group** | An optional group name. Groups let you turn sets of related triggers on and off together (see below). |
| **Priority** | Controls the order triggers are checked in. Lower numbers run first. The default is `10000`. |
| **Character** | If set, the trigger only fires when this character is the active character. Leave blank for the trigger to fire for every character. |
| **Enabled** | Whether the trigger is currently active. |
| **Is Silent** | When checked, the command the trigger sends is not echoed to the terminal. |
| **Disable after Triggered** | The trigger turns itself off after firing once. Useful for one-shot reactions. |
| **Stop Processing** | If this trigger matches, no lower priority triggers will be checked against the same line. |
| **Gag** | Hides the matching line from the terminal. The line still exists in the terminal's buffer, so it is included if you copy text or save the terminal's contents. |
| **Line Transformer** | Rewrites the matching text as it is shown in the terminal instead of sending a command (see below). |
| **Variable Replacement** | Allows `@Variable` references inside the pattern to be replaced with the variable's current value before matching. |
| **Temp** | A temporary trigger that is not saved with the profile. |
| **Package** | The [package](Packages.md) this trigger belongs to, if any. |

The trigger list also shows statistics for each trigger, such as how many times it has fired (**Count**), when it **Last Matched** and its **Max Execution Time**. These are useful when tracking down a trigger that's firing too often or slowing the client down.

## Groups

Placing triggers in a group lets you switch an entire set of behavior on or off at once. For example, you could put all of your combat triggers in a group called `combat` and only turn them on when you're ready to fight:

```
#group-enable combat
#group-disable combat
```

Because these are regular commands, they can be typed by hand, bound to a [macro](Macros.md) or [alias](Aliases.md), or even sent by another trigger.

## Priority and Stop Processing

Triggers are checked in order of their **Priority**, lowest number first. Normally every enabled trigger is checked against every line, so more than one trigger can fire from the same line.

If a trigger has **Stop Processing** checked and it matches, no triggers after it are checked for that line. This is useful when you have a specific trigger that should take precedence over a more general one, and it can also improve performance when you have a large number of triggers. Use it carefully: a high priority trigger with **Stop Processing** set and a loose pattern can quietly prevent your other triggers from ever firing.

## Character Specific Triggers

If you play more than one character in the same game, you can set the **Character** field so that a trigger only fires for that character. For example, a healing trigger for your cleric shouldn't fire when you're logged in as your warrior. Triggers with a blank Character field fire for everyone.

## Gagging Lines

Checking **Gag** hides any line that matches the pattern. A gag trigger doesn't need to do anything else, which makes it ideal for cleaning up spam:

| Field | Value |
|-------|-------|
| Name | Hide weather |
| Pattern | `^The (sun|rain|wind|clouds) ` |
| Gag | Checked |

## Line Transformers

A Line Transformer changes the way a line is displayed instead of sending a command. The text matched by the pattern is replaced with the result of the trigger's command. Line transformers only change what you see; your other triggers still match against the original text from the game.

- With **Execute As** set to `Command`, the matched text is replaced with the command text as written.
- With **Execute As** set to `JavaScript`, the script is run and the string it returns becomes the replacement text.

**Example: Make a tell stand out.**

| Field | Value |
|-------|-------|
| Pattern | `tells you` |
| Command | `{Ytells you{x` |
| Line Transformer | Checked |

Replacement text can include color codes; see [Colors](Colors.md). Line transformers can be turned on or off for the entire profile with the **Replacement Triggers Enabled** profile setting.

## JavaScript Triggers

When **Execute As** is set to `JavaScript`, the command is run as a script instead of being sent to the game. This allows triggers to make decisions, do math, update variables and interact with the client. In addition to `%0`, `%1`, etc., JavaScript triggers can use:

| Placeholder | Value |
|-------------|-------|
| `%Line` | The line that fired the trigger with color codes removed. |
| `%LineRaw` | The line including its formatting codes, useful when copying the line somewhere else while keeping its colors. |

Placeholder values are inserted already escaped for use inside a JavaScript string, so wrap them in quotes:

```javascript
var hp = parseInt("%1");

if (hp < 100) {
    terminal.Send("quaff heal");
}
```

See [Scripting](Scripting.md) for the functions available to scripts.

## Highlight Triggers

Highlight Triggers are a separate, simpler kind of trigger. They don't run commands at all; they change the foreground and background color of any text that matches their pattern. Use them to make important words like your name, a guildmate's name or a rare item jump out of the terminal. See [Highlight Triggers](HighlightTriggers.md) for how they work and how to keep them from slowing the client down.

## Turning All Triggers Off

Triggers can be turned off for an entire profile with the **Triggers Enabled** profile setting. This is handy if a trigger is misbehaving and you need to stop everything at once.

## Troubleshooting

- **My trigger doesn't fire.** Check that it's enabled, that its group hasn't been disabled, that the Character field is blank or matches your current character, that **Triggers Enabled** is on for the profile, and that a higher priority trigger with **Stop Processing** isn't matching the line first. Remember that patterns are case sensitive.
- **I'm not sure which trigger fired.** The `#recent-triggers` command lists the triggers that fired most recently.
- **A trigger is firing in a loop.** If a trigger sends a command that causes the game to print text that matches the same trigger, it will fire again. Tighten the pattern with `^` and `$`, or check **Disable after Triggered**.

[Return to Table of Contents](Index.md)
