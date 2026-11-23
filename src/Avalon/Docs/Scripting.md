# Scripting
---

Scripting enhances the client by allowing automated and advanced customization options. Scripts can send commands to the game, make decisions based on what the game sends you, track your character's state, update the client's windows and much more.

Scripts are written in Mosaic JavaScript, a JavaScript-style language that runs on .NET. This page covers what Avalon adds to that language: the objects, types and functions you use to work with the game and the client. For the language itself (variables, loops, functions, arrays, `include`, LINQ, `async`/`await` and how it differs from browser JavaScript) see the [Scripting Language Reference](Scripting/README.md).

## Contents

- [Where Scripts Run](#where-scripts-run)
- [The Script Environment](#the-script-environment)
- [terminal](#terminal)
- [ui](#ui)
- [db](#db)
- [profile, vm, settings and client](#profile-vm-settings-and-client)
- [Avalon Types](#continuum-types)
- [Extension Methods](#extension-methods)
- [Shared Modules and Types](#shared-modules-and-types)
- [Examples](#examples)
- [Troubleshooting](#troubleshooting)

## Where Scripts Run

JavaScript can be run from many places in the client. Everywhere it runs it has the same objects available, described in [The Script Environment](#the-script-environment).

| Location | When it runs | Notes |
|----------|--------------|-------|
| **Scripts** (Scripts → Edit Scripts, `Ctrl+Alt+X`) | When the profile is opened, and again whenever a script has been changed before the next alias, trigger or event runs. | Scripts run in the **global scope**, so the functions and variables they declare can be used by every other script in the profile. This is the place for shared helper functions. |
| [Aliases](Aliases.md) | When you type the alias and its **Execute As** is `JavaScript`. | `%0`–`%9` are replaced with the words you typed before the script runs. They are inserted as typed, so wrap them in quotes and be careful with words that contain quotes. |
| [Triggers](Triggers.md) | When a line from the game matches the trigger and its **Execute As** is `JavaScript`. | `%0` is the whole line, `%1`, `%2`, ... are the pattern's groups, and `%Line` / `%LineRaw` are the line without and with color codes. These values are already escaped for use inside a JavaScript string. |
| [GMCP](GMCP.md) events | When the game sends a GMCP message. | Avalon calls the function in the Scripts list with the same name as the event and passes it the JSON as a string. |
| [Variables](Variables.md) **On Change Event** | When a variable's value changes. | `%1` is replaced with the old value and `%2` with the new value. |
| [Profile](Profiles.md) **On Profile Loaded Event** | When the profile is opened. | |
| [Profile](Profiles.md) **On Profile Closed Event** | When the profile's terminal is closed or the client exits. | |
| [Profile](Profiles.md) **On Tick Event** | When the tick timer counts down to a game tick. | |
| [Alert](#alert) action links | When you click the action link on an alert that has its `JavaScript` property set. | |
| Scheduled and batch tasks | When a task added with `terminal.AddScheduledTask` or `terminal.AddBatchTask` comes due. | Only when the task was added with `isJavaScript` set to `true`. |

Everything other than the Scripts list runs in its own **scope**: the variables you declare with `let` and `const` there go away when the snippet finishes, but it can still call the functions declared in the Scripts list. To keep a value between runs, store it in a [variable](Variables.md) with `terminal.SetVariable` or in the shared [`globals`](#globals) dictionary.

### One Environment Per Profile

Every open profile has its own script environment. The `terminal`, `profile` and `vm` objects in a script always refer to the profile the script belongs to, so a trigger in one profile can't accidentally send commands to another connection. The script environment is rebuilt from scratch when the profile is reopened, which re-runs every script in the Scripts list.

The one thing shared across every profile is the [`globals`](#globals) dictionary.

### Threading

Scripts run on the client's UI thread, which is what lets them safely change windows, variables, triggers and the other objects described here. A long running loop will make the client unresponsive until it finishes, so:

- Use `await terminal.PauseAsync(ms)` (or `await ui.PauseAsync(ms)`) to wait inside a script. The client keeps working while the script waits.
- `terminal.Pause(ms)` also waits without freezing the window, but it holds up the code that called the script (for example the trigger processing for the current line) until it finishes. Prefer `PauseAsync`.
- To run something later rather than waiting for it, use [scheduled tasks](#tasks).

## The Script Environment

Avalon registers the following names with every profile's script environment. The script editor knows about all of them and will offer completion for their members as you type.

### Objects

| Name | What it is |
|------|------------|
| [`terminal`](#terminal) | The game terminal for this profile: send commands, echo text, read and change variables, work with triggers and terminals, alerts and tasks. Most scripts use this more than anything else. |
| [`ui`](#ui) | The client's main window: toasts, dialog boxes, input boxes, the status bar, window size and themes. |
| [`db`](#db) | The client's SQLite [database](Database.md). |
| [`profile`](#profile) | The profile that is open, including its aliases, triggers, directions, variables and scripts. |
| [`vm`](#vm) | Per-game runtime state that scripts can use to track health, mana, room, effects and available exits. |
| [`settings`](#settings-and-client) | The client-wide settings (the same settings found under Settings on the ribbon). |
| [`client`](#settings-and-client) | Settings that are local to this computer. |
| [`globals`](#globals) | A dictionary shared by every script in every profile. |
| `JSON`, `http`, `hash`, `clipboard`, `regex`, `log`, `environ`, `process`, `mouse`, `screenshot`, `ai`, `globalThis` | General purpose modules. See [Shared Modules and Types](#shared-modules-and-types). |

### Types

These types can be created with `new` and their static members used directly.

| Name | What it is |
|------|------------|
| [`Affect`](#affect) | An entry in the Affects window. |
| [`Alias`](#alias) | An alias. |
| [`Trigger`](#trigger) | A trigger. |
| [`HighlightTrigger`](#highlighttrigger) | A highlight trigger. |
| [`Variable`](#variable) | A profile variable. |
| [`Direction`](#direction) | A saved direction (speedwalk). |
| [`Alert`](#alert) | An alert shown in the alert bar above the terminal. |
| `Profile` | A profile. You'll normally use the `profile` object rather than creating one. |
| `InfoBarSeverity` | The severity of an `Alert`: `Information`, `Success`, `Warning` or `Error`. |
| `string`, `int`, `double`, `date`, `guid`, `math`, `file`, `directory`, `StringBuilder`, `Thickness` | .NET types. See [Shared Modules and Types](#shared-modules-and-types). |

Every type in the `Avalon.Models` namespace can also be reached by its full name, which is how you get to types without a short name such as `Avalon.Models.ExecuteType`:

```javascript
let alias = new Alias();
alias.ExecuteAs = Avalon.Models.ExecuteType.JavaScript;
```

The `System` namespace is available as well, so `System.Text.RegularExpressions.Regex` and the other .NET types in `System` can be used by their full names or imported with [`include`](Scripting/Include.md).

> **Note:** Member names on Avalon objects are the .NET names and are case-sensitive: it's `terminal.Send`, not `terminal.send`, and `vm.Hp`, not `vm.hp`.

## terminal

`terminal` is the game terminal that the script's profile is running in.

Several functions take a **terminal ID**. This is the name of an extra terminal window (see [New Terminal](UI-Elements.md#new-terminal)), for example `"Communication"` or a name you've created such as `"Loot"`. Functions without a terminal ID work on the main game terminal.

### Sending Commands

| Function | Description |
|----------|-------------|
| `Send(cmd)` | Sends a command to the game exactly as if you typed it. It's split on `;`, aliases are expanded, `@variables` are replaced and `#` hash commands are run. The command is echoed to the terminal but isn't added to your input history. |
| `SendRaw(cmd)` | Sends the text to the game as-is with no alias, variable, hash command, speedwalk or `;` processing. |
| `IsConnected()` | Returns `true` if the terminal is currently connected to the game. |
| `ConnectAsync(executeOnConnected = false)` | Connects to the game if not connected. When `executeOnConnected` is `true`, the profile's **On Connect Command** is sent after connecting. Returns `true` if connected. Use with `await`. |
| `Disconnect()` | Disconnects from the game. |

```javascript
terminal.Send("stand;wake;look");
terminal.SendRaw("say I really mean @this;literally");
```

### Writing to Terminals

Text written to a terminal can contain [color codes](Colors.md).

| Function | Description |
|----------|-------------|
| `Echo(text)` | Writes text to the game terminal without adding a line ending. |
| `EchoLine(text)` | Writes a line of text to the game terminal. |
| `EchoNewLine()` | Writes a blank line to the game terminal. |
| `WriteLine(text)` | Writes text to the game terminal without adding a line ending (the same as `Echo`). |
| `WriteLine(terminalId, text, includeTimestamp = true)` | Writes a line to the named terminal, creating and opening the terminal if it doesn't exist. A `[timestamp]` is added to the start of the line unless `includeTimestamp` is `false`. |
| `SetText(terminalId, text)` | Replaces all the text in the named terminal. |
| `Clear()` | Clears the game terminal. |
| `Clear(terminalId)` | Clears the named terminal. |
| `ClearBackBuffer()` | Removes scrollback above the visible game terminal. |
| `ScrollToEnd()` | Scrolls the game terminal to the end. |
| `ScrollToEnd(terminalId)` | Scrolls the named terminal to the end. |
| `TrimEnd()` | Removes trailing line feeds, carriage returns and spaces from the end of the game terminal. |
| `Log(text)` | Writes a message to the [Log Window](UI-Elements.md#log-window) instead of the game terminal. Useful for debugging. |
| `SaveText(terminalId, filePath, includeAnsi = false, lines = -1)` | Saves the named terminal's text to a file. Color codes are removed unless `includeAnsi` is `true`. When `lines` is greater than 0 only that many lines from the end are saved. |
| `SaveTextAsync(terminalId, filePath, includeAnsi = false, lines = -1)` | The same as `SaveText`, for use with `await`. |

```javascript
terminal.EchoLine("{GYou are fully healed.{x");
terminal.WriteLine("Loot", "You picked up a shiny sword.");
terminal.WriteLine("Loot", "No timestamp on this one.", false);
```

### Reading and Changing the Game Terminal

| Function | Description |
|----------|-------------|
| `LastNonEmptyLine()` | Returns the last line in the game terminal that isn't blank. This isn't necessarily the line that fired a trigger; in a trigger, use `%0` or `%Line` for that. |
| `LastLines(count)` | Returns the last `count` lines of the game terminal as an array, oldest first. |
| `LastLines(count, reverseOrder)` | Returns the last `count` lines. When `reverseOrder` is `false` the newest line comes first. |
| `LastLinesBetweenContains(start)` | Returns the last line that contains `start` and every line after it, oldest first, with color codes removed. Returns the whole transcript if `start` is absent. |
| `LastLinesBetweenContains(start, end)` | Returns the lines from the last match for `start` through the next match for `end`. Returns an empty array if `start` is absent, or only the starting line if `end` is absent. |
| `LastLinesBetweenStartsWith(start)` | Like `LastLinesBetweenContains`, but matches lines that *start with* the text. |
| `LastLinesBetweenStartsWith(start, end)` | Like `LastLinesBetweenContains(start, end)`, but matches lines that *start with* the text. |
| `ReplaceLastInstance(search, replacement)` | Replaces the last occurrence of `search` in the game terminal. `replacement` can contain color codes. |
| `ReplaceAll(search, replacement)` | Replaces every occurrence of `search` in the game terminal. |
| `ReplaceAll(terminalId, search, replacement)` | Replaces every occurrence of `search` in the named terminal. |
| `RemoveLine(lineNumber)` | Removes a line from the game terminal. |
| `RemoveLastLine()` | Removes the last line from the game terminal. |
| `RemoveColor(text)` | Returns `text` with all color codes removed. |

```javascript
// Collect the output of the "score" command, from its header to its footer.
let lines = terminal.LastLinesBetweenStartsWith("Score for", "-----");

for (const line of lines) {
    terminal.WriteLine("Score", line, false);
}
```

### Variables

These work with the profile's [variables](Variables.md). Variable values are always strings.

| Function | Description |
|----------|-------------|
| `GetVariable(key)` | Returns the variable's value, or `""` if it doesn't exist. |
| `SetVariable(key, value)` | Sets a variable, creating it if it doesn't exist. If the value changed and the variable has an **On Change Event**, the event runs. |
| `SetVariable(key, value, color)` | Sets a variable and its foreground color (for example `"#FF0000"`) in the Variables window. |
| `RemoveVariable(key)` | Deletes a variable. |
| `ReplaceVariables(text)` | Returns `text` with every `@name` replaced by the variable's value. Unknown variables are left as they are. |
| `ShowVariable(key)` | Shows the variable in the [Variables window](UI-Elements.md#variables). |
| `HideVariable(key)` | Hides the variable from the Variables window. |
| `SetCharacter(name)` | Sets the active character. This sets the `Character` variable and the character name shown in the client, and controls which character-specific aliases and triggers are active. |

```javascript
let gold = int.Parse(terminal.GetVariable("Gold") || "0");
terminal.SetVariable("Gold", (gold + 100).toString());
terminal.EchoLine(terminal.ReplaceVariables("You have @Gold gold."));
```

### Aliases, Triggers and Highlights

| Function | Description |
|----------|-------------|
| `EnableGroup(groupName)` | Enables every alias and trigger whose **Group** matches (not case-sensitive). |
| `DisableGroup(groupName)` | Disables every alias and trigger in the group. |
| `EnableTrigger(id)` | Enables the trigger with the given **ID**. |
| `DisableTrigger(id)` | Disables the trigger with the given **ID**. |
| `Highlight(lookupId, text)` | Highlights `text` in red on black using the [highlight trigger](HighlightTriggers.md) with the given **Lookup Key**, creating the highlight trigger if needed. `text` is a regular expression; separate several values with `\|`. Replaces anything the highlight trigger matched before. |
| `Highlight(lookupId, text, foreground, background)` | The same with your own colors, for example `"#FFFF00"` and `"#000000"`. |
| `HighlightAppend(lookupId, text)` | Adds `text` to the list of values the highlight trigger matches. |
| `HighlightRemove(lookupId, text)` | Removes `text` from the list of values the highlight trigger matches. |
| `HighlightEnable(lookupId)` | Enables the highlight trigger. |
| `HighlightDisable(lookupId)` | Disables the highlight trigger. |

```javascript
// Highlight everyone in the group, adding members as they join.
terminal.Highlight("group", "Rhien|Kael", "#00FFFF", "#000000");
terminal.HighlightAppend("group", "Marisol");
terminal.HighlightRemove("group", "Kael");
```

To work with aliases and triggers directly, use the lists on [`profile`](#profile).

### Alerts

Alerts are shown in a bar above the game terminal.

| Function | Description |
|----------|-------------|
| `AlertInfo(message, title = "Alert", duration = 0)` | Shows an informational alert. `duration` is in seconds; `0` leaves the alert up until it's closed. |
| `AlertSuccess(message, title = "Alert", duration = 0)` | Shows a success alert. |
| `AlertWarning(message, title = "Alert", duration = 0)` | Shows a warning alert. |
| `AlertError(message, title = "Alert", duration = 0)` | Shows an error alert. |
| `Alert(alert)` | Shows an [`Alert`](#alert) you've built yourself, which can include an action link that runs a command or JavaScript. |
| `AlertRemoveByMessage(message)` | Removes alerts with the given message. |
| `AlertCloseAll()` | Closes all alerts. |

```javascript
terminal.AlertWarning("Your sanctuary is about to wear off.", "Sanctuary", 10);
```

### Tasks

Tasks run a command, or JavaScript, later without holding up the script that added them.

| Function | Description |
|----------|-------------|
| `AddScheduledTask(command, isJavaScript, seconds)` | Runs `command` after `seconds` seconds. When `isJavaScript` is `false` it's sent like `Send`; when `true` it's run as JavaScript. |
| `ClearTasks()` | Removes every scheduled task that hasn't run yet. |
| `AddBatchTask(command, isJavaScript)` | Adds a command or script to the batch. Batch tasks don't run until `StartBatch` is called. |
| `StartBatch(secondsInBetweenCommands = 1)` | Runs the batch tasks in the order they were added, waiting the given number of seconds between each one. |

```javascript
terminal.AddScheduledTask("cast 'sanctuary'", false, 120);
terminal.AddScheduledTask('terminal.EchoLine("{YTwo minutes are up.{x")', true, 120);

terminal.AddBatchTask("get all corpse", false);
terminal.AddBatchTask("sacrifice corpse", false);
terminal.StartBatch(2);
```

### Windows and the Client

| Function | Description |
|----------|-------------|
| `AddTerminal(id, containerName, autoHide = false, side)` | Creates a terminal named `id` and docks it with the window named `containerName`. If there's no window by that name, a new group named `containerName` is docked along the `side` of the client (the bottom by default). If the terminal already exists it's activated instead. |
| `Activate(id, focus = false)` | Brings the named document or tool window to the front, and gives it keyboard focus when `focus` is `true`. |
| `SetTitle(title)` | Sets the title of the profile's tab. |
| `FocusInput()` | Moves keyboard focus to the command box. |
| `SetStatus(text, ms = 4000)` | Shows a message in the status bar for `ms` milliseconds. |
| `Pause(ms)` | Waits `ms` milliseconds. See [Threading](#threading). |
| `PauseAsync(ms)` | Waits `ms` milliseconds without holding anything up. Use with `await`. |
| `GetAppDataFilePath(fileName)` | Returns the full path to `fileName` in the client's application data folder. |
| `GetDocumentsFilePath(fileName)` | Returns the full path to `fileName` in the client's documents folder. |
| `GetBrush(hex)` | Returns a color brush for a hex color such as `"#246840"`, for setting colors on objects like `Affect`. |

## ui

`ui` works with the client's main window.

| Function | Description |
|----------|-------------|
| `Toast(title, message)` | Shows a toast notification. |
| `ToastInfo(title, message)` | Shows an informational toast. |
| `ToastWarning(title, message)` | Shows a warning toast. |
| `ToastError(title, message)` | Shows an error toast. |
| `Alert(message)` | Shows a message box. The script waits until it's closed. |
| `InputBox(message)` | Asks for text and returns what was entered. |
| `ShowString(text)` | Shows text in a dialog box, useful for looking at long values while debugging. |
| `StatusText` | Gets or sets the text in the status bar. |
| `FlashWindow()` | Flashes the client in the Windows taskbar to get your attention. |
| `SetWindowDimensions(height, width)` | Sets the size of the main window. |
| `SetWindowPosition(top, left)` | Sets the position of the main window. |
| `SetTheme(themeName)` | Changes the client's [theme](Themes.md). |
| `SetVital(panelName, label, current, maximum)` | Updates or adds a row in a [vitals panel](Layouts.md#vitals-panels). Returns `false` if the panel is missing or the values are invalid. The maximum must be positive. |
| `RemoveVital(panelName, label)` | Removes a vitals row for the current session. Returns whether a row was removed. |
| `SendKeys(keys)` | Simulates key presses. |
| `Pause(ms)` / `PauseAsync(ms)` | The same as the `terminal` versions. |
| `GarbageCollect()` | Asks .NET to free unused memory. |

Vitals panel names and row labels are case-insensitive. Add the panel from **Layout** before calling `ui.SetVital`. A script can add a row on the fly, and **Configure Vitals** saves the panel's labels for future sessions.

```javascript
let name = ui.InputBox("Who should I follow?");

if (name) {
    terminal.Send(`follow ${name}`);
    ui.ToastInfo("Following", name);
}
```

## db

`db` runs SQL against the client's SQLite [database](Database.md). Always pass values as parameters rather than building them into the SQL string. Parameters are numbered from one: `@1` is the first value passed after the SQL, `@2` the second and so on.

> **Note:** `db.Execute` works out how many parameters a statement has by counting the `@` characters in it. Use each parameter once, and don't put an `@` anywhere else in the statement (such as inside a string literal); pass that text as a parameter instead.

| Function | Description |
|----------|-------------|
| `Execute(sql, ...parameters)` | Queues an `INSERT`, `UPDATE`, `DELETE` or other statement. Queued statements are written together every few seconds (the **Database Write Interval** setting), which is fast enough to call from a trigger that fires often. |
| `ExecuteImmediate(sql, ...parameters)` | Runs a statement right away rather than queuing it. |
| `Flush()` / `FlushAsync()` | Writes everything in the queue. Await either method before reading back rows you've just queued. |
| `SelectValue(sql, ...parameters)` | Returns the first column of the first row, or `null`. |
| `SelectValueAsync(sql, ...parameters)` | The same, for use with `await`. |
| `Select(sql, ...parameters)` | Returns the rows as a list of dictionaries keyed by column name. |

```javascript
db.ExecuteImmediate("CREATE TABLE IF NOT EXISTS kills (mob TEXT, killed_at TEXT)");
db.Execute("INSERT INTO kills (mob, killed_at) VALUES (@1, @2)", "%1", date.Now.ToString("s"));

await db.FlushAsync();
let total = db.SelectValue("SELECT COUNT(*) FROM kills WHERE mob = @1", "%1");
terminal.EchoLine(`You have killed %1 ${total} times.`);

for (const row of db.Select("SELECT mob, COUNT(*) AS n FROM kills GROUP BY mob ORDER BY n DESC LIMIT 5")) {
    terminal.EchoLine(`${row["mob"]}: ${row["n"]}`);
}
```

## profile, vm, settings and client

These objects are the client's own live data. Changing a property changes it in the client immediately, and changes to `profile` and `settings` are saved along with everything else.

### profile

`profile` is the open [profile](Profiles.md). Its most useful members are:

| Member | Description |
|--------|-------------|
| `Name`, `HostName`, `Port` | The profile's name and the game it connects to. |
| `AliasList` | The profile's [`Alias`](#alias) objects. |
| `TriggerList` | The profile's [`Trigger`](#trigger) objects. |
| `HighlightTriggers` | The profile's [`HighlightTrigger`](#highlighttrigger) objects. |
| `DirectionList` | The profile's [`Direction`](#direction) objects. |
| `Variables` | The profile's [`Variable`](#variable) objects. `terminal.GetVariable` and `SetVariable` are usually easier. |
| `MacroList` | The profile's [macros](Macros.md). |
| `Scripts` | The scripts in the Scripts list. |
| `AutoCompleteList` | Extra words offered by the command box's auto complete. |
| `TriggersEnabled`, `ReplacementTriggersEnabled`, `AliasesEnabled`, `GmcpEnable` | Turn triggers, line transformers, aliases and GMCP on or off for the whole profile. |
| `OnConnectCommand`, `OnProfileLoadedEvent`, `OnProfileClosedEvent`, `OnTickEvent` | The profile's event commands and scripts. |
| `TickDurationInSeconds`, `EchoTickWarning` | Tick timer settings. |
| `StatsBarVisible`, `StatsBarExtendedInfo`, `StatsBarTickVisible` | Stats bar settings. |
| `SaveLastCommand` | Whether your last command is saved to the `LastCommand` variable. |

```javascript
// Turn off every trigger that belongs to a character other than the current one.
let me = terminal.GetVariable("Character");

for (const tr of profile.TriggerList) {
    if (tr.Character != "" && tr.Character != me) {
        tr.Enabled = false;
    }
}
```

### vm

`vm` holds character state for the current game. [GMCP](GMCP.md) event scripts and triggers can populate it, and other scripts can read it. Avalon keeps this state in memory.

| Member | Description |
|--------|-------------|
| `Character` | The character name shown in the client. Use `terminal.SetCharacter` to change it. |
| `Hp`, `MaxHp`, `Mana`, `MaxMana`, `Move`, `MaxMove` | Health, mana and movement. |
| `Level`, `Str`/`MaxStr`, `Int`/`MaxInt`, `Wis`/`MaxWis`, `Dex`/`MaxDex`, `Con`/`MaxCon`, `Tha`/`MaxTha` | Level and attributes. |
| `RoomName`, `Exits` | The current room and its exits. |
| `Stance`, `IsFighting`, `IsRiding`, `IsAfk`, `IsQuiet` | Character state. |
| `WiziLevel`, `IncogLevel` | Immortal visibility levels. |
| `Tick` | A script-populated tick countdown. |
| `Affects` | The current effects. Call `AddOrUpdateAffect` and `RemoveAffect` to update the collection by name. |
| `Movement` | The available exits. Set its direction properties or pass an array of directions to `SetMovement`. |
| `IsRecording` | Whether commands are being recorded for a new direction. |
| `ScriptsActive` | An estimate of how many scripts are running right now. |

```javascript
function char_vitals(json) {
    let obj = JSON.parse(json);
    vm.Hp = obj.hp;
    vm.MaxHp = obj.maxhp;
    vm.Mana = obj.mana;
    vm.MaxMana = obj.maxmana;
}
```

### settings and client

`settings` is the client-wide settings object, the same settings shown under Settings on the ribbon. A few that scripts commonly read or change are `SpeedwalkCharacter`, `SpamGuard`, `SpamGuardCommand`, `BeepEnabled`, `BackBufferEnabled`, `WordWrapTerminals`, `FontSize` and `ApplicationDataFolder`.

`client` holds the settings that are stored on this computer only, such as the folder used by `terminal.GetDocumentsFilePath`.

```javascript
settings.SpamGuard = true;
settings.SpamGuardCommand = "look";
```

## Avalon Types

### Affect

An entry in the [Affects](UI-Elements.md#affects) window.

| Property | Description |
|----------|-------------|
| `Name` | The name of the affect, such as `"sanctuary"`. |
| `Duration` | The time left. `-1` means unknown. |
| `MaxDuration` | The longest duration seen, used to draw the progress bar. |
| `Modifies`, `Modifier` | What the affect changes and by how much, for example `"hitroll"` and `3`. |
| `Permanent`, `UnknownDuration`, `IsSong` | Flags that change how the affect is displayed. |
| `Background`, `Foreground` | Colors; use `terminal.GetBrush("#hex")`. |

```javascript
let af = new Affect();
af.Name = "sanctuary";
af.Duration = 12;
vm.Affects.AddOrUpdateAffect(af);
```

### Alias

| Property | Description |
|----------|-------------|
| `Id` | A unique identifier, created automatically. |
| `AliasExpression` | What you type to run the alias. |
| `Command` | The command(s) or JavaScript to run. |
| `ExecuteAs` | `Avalon.Models.ExecuteType.Command` or `Avalon.Models.ExecuteType.JavaScript`. |
| `Group`, `Character`, `Enabled` | Same as in the [alias editor](Aliases.md). |
| `ButtonLabel`, `ButtonVisible`, `SortOrder` | How the alias appears in the [Alias Button Actions](UI-Elements.md) window. |
| `Count` | The number of times the alias has been used. |

```javascript
let a = new Alias();
a.AliasExpression = "gg";
a.Command = "get gold corpse;split all";
profile.AliasList.Add(a);
```

### Trigger

| Property | Description |
|----------|-------------|
| `Id` | A unique identifier, created automatically. Used by `terminal.EnableTrigger` and `DisableTrigger`. |
| `Name` | A friendly name. |
| `Pattern` | The regular expression to match. An invalid pattern is ignored. |
| `Command` | The command(s) or JavaScript to run. |
| `ExecuteAs` | `Avalon.Models.ExecuteType.Command` or `Avalon.Models.ExecuteType.JavaScript`. |
| `Group`, `Character`, `Enabled`, `Priority` | Same as in the [trigger editor](Triggers.md). |
| `IsSilent`, `Gag`, `StopProcessing`, `DisableAfterTriggered`, `VariableReplacement`, `LineTransformer` | Same as in the trigger editor. |
| `Temp` | When `true` the trigger isn't saved with the profile. Good for triggers that scripts create. |
| `Count`, `LastMatched` | How many times and when the trigger last fired. |

```javascript
let tr = new Trigger();
tr.Name = "Follow the leader";
tr.Pattern = "^(\\w+) leaves (north|south|east|west|up|down)\\.$";
tr.Command = "%2";
tr.Temp = true;
profile.TriggerList.Add(tr);
profile.SortTriggers();
```

> **Note:** Set `Pattern` to a valid regular expression. If it isn't valid the pattern isn't changed, and because a trigger created in a script isn't linked to a terminal, the error is written to the [Log Window](UI-Elements.md#log-window) rather than the game terminal.

### HighlightTrigger

| Property | Description |
|----------|-------------|
| `LookupKey` | The name used by the `terminal.Highlight` functions. |
| `Pattern` | The regular expression to color. |
| `ForegroundColor`, `BackgroundColor` | Colors; use `terminal.GetBrush("#hex")`. |
| `Enabled` | Whether the highlight is active. |

The `terminal.Highlight` functions are usually easier than creating these yourself.

### Variable

| Property | Description |
|----------|-------------|
| `Key`, `Value` | The variable's name and value. |
| `Label` | The label shown in the Variables window in place of the key. |
| `IsVisible`, `DisplayOrder` | Whether and where it appears in the Variables window. |
| `ForegroundColor`, `BackgroundColor` | Hex colors used in the Variables window. |
| `Character` | The character the variable belongs to. |
| `OnChangeEvent` | JavaScript to run when the value changes. |

`new Variable(key, value)` and `new Variable(key, value, color)` create a variable in one step, but `terminal.SetVariable` is the usual way to create one.

### Direction

| Property | Description |
|----------|-------------|
| `Id` | A unique identifier, created automatically. |
| `Name` | The name you walk to. |
| `Speedwalk` | The speedwalk path, for example `"3n2e"`. |
| `StartingRoom`, `EndingRoom` | The room names the direction starts and ends in. See [Directions](Directions.md). |

### Alert

Build an `Alert` when the `terminal.Alert*` shortcuts aren't enough, for example to add an action link.

| Property | Description |
|----------|-------------|
| `Title`, `Message` | The alert's text. |
| `Severity` | `InfoBarSeverity.Information`, `Success`, `Warning` or `Error`. |
| `Duration` | Seconds before the alert closes itself. |
| `Permanent` | When `true` the alert stays until it's closed. |
| `ActionMessage` | The text of an action link shown on the alert. |
| `Command` | A command to send when the action link is clicked. |
| `JavaScript` | JavaScript to run when the action link is clicked. |

```javascript
let alert = new Alert();
alert.Title = "Tell";
alert.Message = "Rhien wants to group.";
alert.Severity = InfoBarSeverity.Information;
alert.Permanent = true;
alert.ActionMessage = "Accept";
alert.Command = "group Rhien";
terminal.Alert(alert);
```

## Extension Methods

Avalon adds these methods to its objects for use from scripts:

| Method | Description |
|--------|-------------|
| `vm.Affects.AddOrUpdateAffect(affect)` | Adds an affect, or if one with the same name exists, updates its duration. The list is then sorted by time left. |
| `vm.Affects.RemoveAffect(name)` | Removes the affect with the given name (not case-sensitive). |
| `vm.Affects.SortAffects()` | Sorts the affects by time left. |
| `vm.Affects.ScrollToLastAffect()` | Scrolls the Affects window to the bottom. |
| `vm.Movement.SetMovement(dirs)` | Sets the movement panel from an array of directions such as `["N", "E", "U"]`, turning the rest off. Valid values are `N`, `S`, `E`, `W`, `NE`, `NW`, `SE`, `SW`, `U` and `D`. |
| `profile.SortTriggers()` | Sorts the trigger list by `Priority`. Call this after adding a trigger with a priority. |
| `profile.FirstOrDefault(id)` | Returns the alias, trigger or direction with the given ID, or `null`. |
| `text.RemoveAnsiCodes()` | Returns a string with its color codes removed. |

```javascript
function room_data(json) {
    let obj = JSON.parse(json);
    vm.RoomName = obj.room;
    vm.Movement.SetMovement(obj.exits);
}
```

## Shared Modules and Types

These are general purpose, rather than being specific to playing a game.

### Modules

| Name | Description |
|------|-------------|
| `globals` | A dictionary shared by every script in every open profile. Values last until the client exits. |
| `JSON` | `JSON.parse(text)` and `JSON.stringify(value)`. |
| `http` | `Get(url)`, `Post(url, data)`, `Put(url, data)` and `Delete(url)`, each with an optional header key and value. They return the response body as a string. Also `UrlEncode` and `UrlDecode`. |
| `hash` | `MD5`, `SHA1`, `SHA256`, `SHA384`, `SHA512`, `CRC32`, `EncodeBase64`, `DecodeBase64`, `UrlEncode` and `UrlDecode`. |
| `clipboard` | `GetText()`, `SetText(text)`, `SetUnicodeText(text)`, `SetHtml(html)`, `ContainsText()` and `Flush()`. |
| `regex` | `IsMatch(input, pattern)`, plus `ListContainsValue`, `ListAddValue` and `ListRemoveValue` for working with `a\|b\|c` style patterns. |
| `log` | Writes to the Log Window at a level: `Debug`, `Info`, `Warning`, `Error` and `Success`. |
| `environ` | Folders (`DesktopFolder()`, `DocumentsFolder()`, `ApplicationDataFolder()`, ...), `MachineName()`, `Username()`, `GetEnvironmentVariable(name)`, `SetEnvironmentVariable(name, value)`, `TickCount()` and `PathCombine(...)`. |
| `process` | `Start(filePath, args)` to run a program, plus `GetProcesses()`, `IsProcessRunning(name, exactMatch)`, `SetFocus(...)`, `WindowTitle(returnParent)`, `GetPosition(...)` and `SetPosition(...)`. |
| `mouse` | `SetPosition(x, y)`, `X`, `Y`, clicks (`LeftClick()`, `RightClick()`, `MiddleClick()`, `LeftDoubleClick()`, and the `Down`/`Up` versions) and `ScrollUp(clicks)`/`ScrollDown(clicks)`. |
| `screenshot` | `CurrentWindow()`, `PrimaryScreen()` and `ByLocation(x, y, width, height)`, which return images. |
| `ai` | Talks to a local model through [Ollama](https://ollama.com): `SetModel(name)`, `SetSystemPrompt(text)`, `await ai.AskAsync(message)` (optionally with the name of the person you're talking to, so each person gets their own conversation), `SetState`, `ClearHistory()` and `ClearAll()`. |
| `globalThis` | The global scope, as in standard JavaScript. |

```javascript
// Remember the last person who sent a tell across every open profile.
globals["LastTell"] = "%1";

// In another profile:
if (globals.ContainsKey("LastTell")) {
    terminal.Send(`reply ${globals["LastTell"]}`);
}
```

### .NET Types

| Name | .NET type | Example |
|------|-----------|---------|
| `string` | `System.String` | `string.IsNullOrWhiteSpace(x)`, `string.Join(", ", items)` |
| `int` | `System.Int32` | `int.Parse("42")` |
| `double` | `System.Double` | `double.Parse("1.5")` |
| `date` | `System.DateTime` | `date.Now`, `date.Now.ToString("HH:mm")` |
| `guid` | `System.Guid` | `guid.NewGuid().ToString()` |
| `math` | `System.Math` | `math.Max(a, b)`, `math.Round(x, 2)` |
| `file` | `System.IO.File` | `file.Exists(path)`, `file.AppendAllText(path, text)` |
| `directory` | `System.IO.Directory` | `directory.CreateDirectory(path)` |
| `StringBuilder` | `System.Text.StringBuilder` | `new StringBuilder()` |
| `Thickness` | `System.Windows.Thickness` | `new Thickness(4)` |

LINQ methods such as `Where`, `Select` and `FirstOrDefault` are available on .NET collections like `profile.TriggerList`. See [LINQ queries](Scripting/LINQ.md).

```javascript
let disabled = profile.TriggerList.Where(t => !t.Enabled).Count();
terminal.EchoLine(`${disabled} triggers are disabled.`);
```

## Examples

### A Shared Helper Function

Add this to the Scripts list so every alias and trigger in the profile can use it:

```javascript
function percent(value, max) {
    if (max <= 0) {
        return 0;
    }

    return math.Round((value / max) * 100);
}
```

Then a trigger with the pattern `^<(\d+)/(\d+)hp` and **Execute As** set to `JavaScript` can use it:

```javascript
let hp = percent(parseInt("%1"), parseInt("%2"));

if (hp < 30 && terminal.GetVariable("Fleeing") != "true") {
    terminal.SetVariable("Fleeing", "true");
    terminal.Send("flee");
    terminal.AlertError(`Fled at ${hp}% health!`, "Low Health", 10);
}
```

### Logging Tells to a Terminal and a File

A trigger with the pattern `^(\w+) tells you '(.*)'$`:

```javascript
terminal.WriteLine("Tells", "%Line");

let path = terminal.GetDocumentsFilePath("tells.log");
file.AppendAllText(path, `${date.Now.ToString("s")} %0\r\n`);

if (!settings.BeepEnabled) {
    ui.FlashWindow();
}
```

### Waiting Between Commands

An alias that waits without freezing the client:

```javascript
terminal.Send("cast 'armor'");
await terminal.PauseAsync(1500);
terminal.Send("cast 'bless'");
await terminal.PauseAsync(1500);
terminal.Send("cast 'sanctuary'");
```

### A Toggle

An alias that turns a group of triggers on and off and remembers which it did:

```javascript
if (terminal.GetVariable("AutoLoot") == "on") {
    terminal.DisableGroup("AutoLoot");
    terminal.SetVariable("AutoLoot", "off", "#FF0000");
} else {
    terminal.EnableGroup("AutoLoot");
    terminal.SetVariable("AutoLoot", "on", "#00FF00");
}

terminal.ShowVariable("AutoLoot");
```

## Troubleshooting

- **Check the Log Window.** Script errors are reported there and in the game terminal, along with the name of the trigger or alias that failed.
- **Names are case-sensitive.** `terminal.send(...)` and `vm.hp` don't exist; use `terminal.Send(...)` and `vm.Hp`.
- **Variable values are strings.** Convert them with `int.Parse`, `double.Parse` or `parseInt` before doing math, and back with `toString()` before calling `SetVariable`.
- **Quote placeholders.** `%1`, `%Line` and the other placeholders are replaced with plain text before the script runs, so write `"%1"`, not `%1`.
- **Functions from the Scripts list aren't found.** Make sure the script is saved and has no errors. If one script in the list fails, the scripts after it may not have loaded; fix the error and reopen the profile.
- **`console.log`, `alert` and `document` aren't available.** Use `terminal.EchoLine`, `terminal.Log`, `ui.Alert` and the other functions on this page. See [Compatibility and troubleshooting](Scripting/Compatibility.md) for other differences from browser JavaScript.

[Return to Table of Contents](Index.md)
