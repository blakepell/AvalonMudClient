# Avalon scripting primer for AI assistants

Read this before writing any JavaScript for an Avalon alias, trigger, variable or script. For more detail call
`get_scripting_guide` with one of the topics listed at the end.

## The language

Avalon scripts are **Mosaic JavaScript**: JavaScript syntax interpreted on .NET. It is *not* a browser or Node.js.

- Declare with `let` / `const` (block scoped) or `var`. Always declare before use; there is no hoisting, so declare
  functions above the code that calls them.
- Supported: `if`/`else`, `?:`, `switch` (put `default` last, `break` explicitly), `for`, `for...of`, `for...in`,
  `while`, `do...while`, `break`, `continue`, `return`, functions, arrow functions, default and rest parameters,
  template literals (`` `HP: ${hp}` ``), arrays and object literals, `try`/`catch`/`finally`/`throw`,
  `async`/`await`, top-level `await`, `new` on exposed .NET types.
- **Not available**: classes, `this`, prototypes, generators/`yield`, modules/`import`/`require`, labels, `with`,
  regex literals (`/abc/` won't work; use .NET `System.Text.RegularExpressions.Regex` or the `regex` module),
  `console`, `window`, `document`, `fetch`, `setTimeout`, `Promise`, `Date`, `RegExp`, `Map`, `Set`,
  `Object.keys`, `Array.isArray`.
- `&&`, `||` and `??` evaluate **both** sides. Guard side effects with `if`, not `a && doThing()`.
- Object spread and destructuring are partial. Copy properties explicitly and use plain member access.
- Caught errors are .NET exceptions: use `error.Message`, not `error.message`.
- Strings are .NET strings: `s.Length`, `s.Trim()`, `s.ToUpper()`, `s.Contains("x")`, `s.Split(' ')`,
  `string.IsNullOrWhiteSpace(s)`. JavaScript array methods (`push`, `map`, `filter`, `reduce`, `join`) work on
  array literals. .NET collections such as `profile.TriggerList` support LINQ: `.Where(t => t.Enabled).Count()`.
- Numbers: use `parseInt("42")`, `int.Parse("42")` or `double.Parse("1.5")`. .NET math is `math.Max`,
  `math.Round` (note the capitals), not `Math.max`.
- **Member names on Avalon objects are .NET names and case-sensitive**: `terminal.Send`, `vm.Hp`, not
  `terminal.send` or `vm.hp`.

## Where code runs and how it is scoped

| Where | Scope and inputs |
|-------|------------------|
| **Scripts list** (no GMCP package) | Runs in the profile's **global scope** when the profile opens and again after any script changes. Functions and top-level variables declared here can be called from every alias, trigger and event in the profile. Keep these to declarations (functions, constants), because top-level commands re-run on every reload. Function names must be unique across all scripts in the profile, or the later one wins. |
| **Scripts list with a GMCP package** | Runs in its own child scope each time that package arrives, with `package` and `json` (the message as a JSON string) defined. Parse with `JSON.parse(json)`. |
| **Script named after a GMCP package** (no GMCP package set) | Avalon calls the function whose name is the package with `.` replaced by `_` (Char.Vitals → `Char_Vitals(json)`). |
| **Alias** (Execute As = JavaScript) | Its own child scope. `%0` = everything typed after the alias, `%1`..`%9` = each word. |
| **Trigger** (Execute As = JavaScript) | Its own child scope. `%0` = the whole line, `%1`..`%9` = the regex groups, `%Line` / `%LineRaw` = the line without / with color codes. Named groups such as `(?<Target>\w+)` also set the variable `Target` before the code runs; .NET numbers named groups after the unnamed ones, so check placeholders with `test_trigger_pattern`. |
| **Trigger, line transformer** (JavaScript) | Evaluated as an **expression**; its string value replaces the matched text. |
| **Variable On Change Event** | `%0` = key, `%1` = old value, `%2` = new value. |
| **Profile events** (On Profile Loaded / Closed / Tick) | Their own child scope. |

Placeholders (`%1`, `%Line`, ...) are replaced with text **before** the code is parsed, already escaped for use
inside a JavaScript string. Always put them in quotes: `let name = "%1";`. A missing argument becomes an empty
string, so `let n = %1;` is a syntax error when the player types nothing; use `parseInt("%1")`.

`let`/`const` in an alias, trigger or event disappear when it finishes. To keep state between runs:

- **Profile variables** (saved with the profile, always strings): `terminal.GetVariable("Key")` returns `""` if
  missing; `terminal.SetVariable("Key", value.toString())`. Commands reference them as `@Key`.
- **`globals`**: a .NET dictionary shared by every profile until the client exits:
  `globals["Key"] = value; if (globals.ContainsKey("Key")) { ... }`.
- **Top-level `var`/`let` in the Scripts list** live in the global scope until the profile reopens.

Variables can be scoped to a character. When a variable exists both shared (no character) and for the current
character (the `Character` variable), the character's one wins.

## Limits

- Scripts run on the UI thread. **Each run is cancelled after 5 seconds.** Never write unbounded loops.
- Wait with `await terminal.PauseAsync(ms)`; keep the total under the limit. For anything later, schedule it:
  `terminal.AddScheduledTask("cast 'armor'", false, 30)` (command) or
  `terminal.AddScheduledTask('my_function()', true, 30)` (JavaScript).
- Commands sent with `terminal.Send` from a script are queued and go out after the script finishes.

## Most used Avalon objects

```
terminal.Send("stand;look")          // commands, split on ';', aliases and @variables expanded
terminal.SendRaw("say literal @text") // no processing
terminal.EchoLine("{GHealed!{x")      // write to the terminal; {G green, {R red, {Y yellow, {x reset
terminal.WriteLine("Tells", text)     // write to a named terminal window (created if needed)
terminal.Log("debug text")            // Log Window
terminal.GetVariable(key) / SetVariable(key, value[, "#hexColor"]) / RemoveVariable(key)
terminal.ShowVariable(key) / HideVariable(key)
terminal.EnableGroup(name) / DisableGroup(name)         // aliases and triggers by Group
terminal.EnableTrigger(id) / DisableTrigger(id)
terminal.Highlight(lookupKey, "word1|word2"[, fg, bg]) / HighlightAppend / HighlightRemove
terminal.AlertInfo(msg, title, seconds) / AlertWarning / AlertError / AlertSuccess
terminal.LastLines(count) / LastNonEmptyLine() / LastLinesBetweenStartsWith(start, end)
terminal.SetCharacter(name) / terminal.IsConnected()
await terminal.PauseAsync(ms)
ui.ToastInfo(title, msg) / ui.InputBox(prompt) / ui.FlashWindow() / ui.StatusText
vm.Hp, vm.MaxHp, vm.Mana, vm.MaxMana, vm.Move, vm.MaxMove, vm.RoomName, vm.IsFighting, vm.Affects, vm.Movement
profile.AliasList, profile.TriggerList, profile.Variables, profile.Scripts   // .NET collections (LINQ)
db.Execute(sql, ...params) / db.Select(sql, ...params) / db.SelectValue(...)  // SQLite, @1 @2 parameters
JSON.parse / JSON.stringify, globals, http.Get(url), file, date.Now, math, int, double, string, guid
```

## Examples

A function in the Scripts list:

```javascript
function percent(value, max) {
    if (max <= 0) {
        return 0;
    }
    return math.Round((value / max) * 100);
}
```

A JavaScript trigger on `^You have (\d+)/(\d+) hit points` that uses it:

```javascript
let hp = percent(parseInt("%1"), parseInt("%2"));
if (hp < 30 && terminal.GetVariable("Fleeing") != "true") {
    terminal.SetVariable("Fleeing", "true");
    terminal.Send("flee");
}
```

A JavaScript alias that toggles a group and remembers the state:

```javascript
if (terminal.GetVariable("AutoLoot") == "on") {
    terminal.DisableGroup("AutoLoot");
    terminal.SetVariable("AutoLoot", "off", "#FF0000");
} else {
    terminal.EnableGroup("AutoLoot");
    terminal.SetVariable("AutoLoot", "on", "#00FF00");
}
```

A command alias needs no JavaScript: alias `kk`, command `kill %1;bash %1`, or command `cast 'fireball' @Target`.

## Writing triggers against real game output

Never guess what the game prints. Look at it first, in small pieces:

1. `get_command_output("stats")` returns what the game sent after the player last typed `stats`. If it hasn't
   been sent, ask the player to type it once. `search_output` finds other example lines (deaths, experience,
   menus) with a few lines of context, and `get_output_lines` widens a block when you need more.
2. Copy the exact lines into `test_trigger_pattern` and adjust the pattern until the placeholders hold what you
   expect. Watch for padding, punctuation and lines that wrap.
3. Save the trigger. The last line of a block is often the prompt; don't anchor a pattern to it unless that's the
   point.

**Storing values in variables needs no JavaScript.** Named groups set the variable of the same name every time the
trigger matches, so a Command trigger with an **empty command** is enough:

```
Pattern: ^Str: (?<Str>\d+)\((?<StrMax>\d+)\)\s+Int: (?<Int>\d+)\((?<IntMax>\d+)\)
Command: (empty)
```

Use one trigger per line of the output, and give the triggers the same group (for example `Stats`) so the
player can turn them on and off together. A group that is empty in a match doesn't change its variable.

**Multi-line blocks** (score sheets, inventories, `who` lists) where a line only means something inside the
block: put the per-line triggers in a group that starts **disabled**, add a trigger on the block's first line
with the command `#group-enable ScoreLines`, and one on its last line (or the prompt) with
`#group-disable ScoreLines`. Keep the start and end triggers out of that group. This keeps the line triggers from firing on similar text elsewhere. Avoid one big
JavaScript trigger that tries to parse the whole block from `terminal.LastLines()`.

## Prefer the simplest thing

Use a **Command** alias or trigger when the action is just sending commands (`;`-separated, with `%1` and
`@Variable`). Use JavaScript only for decisions, math or state. Put shared logic in one Scripts list function and
call it from short aliases and triggers.

## Guide topics

`get_scripting_guide` topics: `primer` (this page), `avalon` (every Avalon object and function), `aliases`,
`triggers`, `variables`, `gmcp`, `hash_commands`, `colors`, and the language reference: `language`,
`getting_started`, `variables_and_operators`, `control_flow`, `functions`, `arrays_and_objects`,
`errors_and_async`, `include`, `linq`, `compatibility`, `recipes`, `application_objects`.
