# Aliases

Aliases are custom command shortcuts that allow you to simplify frequently used actions by mapping short commands to longer sequences of commands or JavaScript code. This section provides comprehensive information about creating, configuring, and managing aliases.

## Overview

An alias acts as a command abbreviation or macro that, when typed in the input box, executes either a series of game commands or runs JavaScript code. Aliases are essential for streamlining gameplay, reducing repetitive typing, and creating powerful automation workflows.

## Properties

The following properties are available when creating or editing an alias:

### Alias

**Type:** String  
**Required:** Yes

The trigger text that activates the alias. This is what you type into the input box to execute the alias. The alias expression is case-sensitive and must be unique within your profile.

**Example:**
```
gs
```

When you type `gs` in the input box, the associated command or script will execute.

---

### Command

**Type:** String  
**Required:** Yes

The command(s) or script that will be executed when the alias is triggered. The behavior of this field depends on the **Execute As** property:

- When **Execute As** is set to **Command**, this field contains one or more game commands separated by semicolons (`;`).
- When **Execute As** is set to **JavaScript**, this field contains JavaScript code that will be executed by the scripting engine.

**Example (Command):**

```
get all.sword;wear all.sword;wield sword
```

**Example (JavaScript):**

```javascript
terminal.Send("cast 'cure light' " + terminal.GetVariable("Character"));
```

---

### Execute As

**Type:** ExecuteType Enum  
**Default:** Command

Determines how the alias will be executed. This property has two possible values:

#### Command
When set to **Command**, the content in the **Command** field is treated as one or more game commands that will be sent directly to the game server. Multiple commands can be separated by semicolons (`;`), and each command will be processed sequentially through the interpreter.

Use **Command** execution when:
- You want to send text directly to the game
- You need to execute multiple game commands in sequence
- You don't need programmatic logic or variable manipulation

**Example:**

```
Alias: bank
Command: open pack;get all.gold pack;deposit all;close pack
Execute As: Command
```

#### JavaScript
When set to **JavaScript**, the content in the **Command** field is interpreted as JavaScript code that will be executed by the Topaz scripting engine. This provides access to the full scripting API, allowing for complex logic, conditional execution, variable manipulation, and interaction with the client.

Use **JavaScript** execution when:
- You need conditional logic or loops
- You want to manipulate variables or perform calculations
- You need access to the client's API functions
- You require dynamic command generation based on game state

**Example:**

```
Alias: heal
Execute As: JavaScript
```

```javascript
if (terminal.GetVariable("hp") < 100) { terminal.Send("cast 'cure light'"); }
```

**Important:** JavaScript aliases have access to the global `terminal` object which provides methods like `terminal.Send()` for sending commands to the game, `terminal.Echo()` for displaying messages to the terminal, and access to variables through `terminal.GetVariable()` and `terminal.SetVariable()`. See [Scripting](Scripting.md) for complete API documentation.

---

### Character

**Type:** String  
**Default:** Empty

Restricts the alias to execute only when a specific character is logged in. This relies on the global `Character` variable being populated with the current character's name. When this field is empty, the alias will work for all characters.

This is useful when you have multiple characters with different needs or abilities and want character-specific aliases.

This requires that the currently logged in character be set.  This be done manually by clicking the `Set Character Name` button on the status bar at the bottom of the mud client.

![Image](data:image/jpeg;base64,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)

**Example:**

```
Character: Gandalf
```

This alias will only trigger when playing the character named "Gandalf".

---

### Group

**Type:** String  
**Default:** Empty

Assigns the alias to a named group. Groups allow you to organize related aliases and enable or disable entire batches of functionality at once. This is particularly useful for organizing aliases by purpose (combat, travel, crafting) or by package.

**Example:**

```
Group: Combat
```

You can then enable or disable all "Combat" aliases as a group through the group management interface.

---

### Enabled

**Type:** Boolean  
**Default:** True

Determines whether the alias is currently active. When set to `false`, the alias will not trigger even if its expression is typed. This allows you to temporarily disable aliases without deleting them.

**Example Use Cases:**

- Temporarily disable debugging aliases in production gameplay
- Turn off aliases that conflict with game commands in specific areas
- Disable outdated aliases while keeping them for reference

---

### Sort Order

**Type:** Integer  
**Default:** 0

Specifies the display order when aliases are rendered in UI views such as button repeaters or list views. Lower numbers appear first. This property does not affect the functionality of the alias, only its visual organization.

**Example:**

```
Sort Order: 10
```

---

### Package

**Type:** PackageMetadata  
**Default:** Null

Associates the alias with a specific package. Packages allow you to bundle related aliases, triggers, scripts, and other client resources together for organization, sharing, and distribution. When an alias belongs to a package, it can be exported, imported, or managed as part of that package.

**Example:**

```
Package: CombatSystem
```

---

### Button Label

**Type:** String  
**Default:** Empty

The text displayed on the button when **Button Visible** is set to `true`. If this field is empty but the button is visible, the system may use the alias expression as the button label.

**Example:**

```
Button Label: Quick Heal
```

---

### Button Visible

**Type:** Boolean  
**Default:** False

Determines whether a clickable button for this alias appears on the alias actions panel. When set to `true`, you can execute the alias by clicking the button instead of typing the alias expression.

This is useful for:
- Frequently used commands you want quick access to
- Aliases with complex or hard-to-remember expressions
- Creating custom toolbar buttons for common actions

---

### Count

**Type:** Integer  
**Default:** 0  
**Read-Only:** Yes

Tracks the number of times the alias has been executed. This is a read-only field that increments automatically each time the alias runs. It can be useful for:
- Identifying frequently used aliases
- Performance monitoring
- Usage statistics and optimization

---

## Command vs. JavaScript Execution

Understanding when to use **Command** versus **JavaScript** execution is crucial for effective alias creation:

### Command Execution

**Advantages:**
- Simple and straightforward syntax
- Direct communication with the game server
- No scripting knowledge required
- Faster execution for simple command sequences
- Ideal for static, unchanging command sequences

**Limitations:**
- No conditional logic
- Cannot access or manipulate variables
- No looping or repetition control
- Cannot interact with client API
- Limited to predefined command sequences

**Best Practices:**
```
Alias: eq
Command: remove all;wear all;wield sword;hold shield
Execute As: Command
```

### JavaScript Execution

**Advantages:**
- Full programming capabilities (conditions, loops, functions)
- Access to client API through the `terminal` object
- Variable manipulation and calculation
- Dynamic command generation
- Complex automation and logic
- Integration with other scripts and systems

**Limitations:**
- Requires JavaScript programming knowledge
- Slightly slower execution due to script interpretation
- Potential for scripting errors if code is incorrect
- More complex to debug

**Best Practices:**
```javascript
Alias: smartheal
Execute As: JavaScript
```

```javascript
var hp = parseInt(terminal.GetVariable("hp"));
var maxhp = parseInt(terminal.GetVariable("maxhp"));

if (hp < maxhp * 0.5) {
    terminal.Send("cast 'heal'");
} else if (hp < maxhp * 0.8) {
    terminal.Send("cast 'cure serious'");
} else {
    terminal.Send("cast 'cure light'");
}
```

---

## Arguments and Parameters

Aliases support passing arguments from the command line to both Command and JavaScript execution types. This allows you to create flexible, reusable aliases.

### Command Execution Arguments

When **Execute As** is set to **Command**, you can use the following placeholder syntax:

- `%0` - Represents the entire argument string after the alias name
- `%1` through `%9` - Individual space-separated arguments

**Example:**
```
Alias: c
Command: cast '%1' %2
Execute As: Command
```

**Usage:**
- Type: `c fireball target` 
- Result: Sends `cast 'fireball' target` to the game
- `%1` = "fireball"
- `%2` = "target"

If the alias command doesn't contain `%` placeholders, all arguments are appended to the end of the command automatically.

### JavaScript Execution Arguments

When **Execute As** is set to **JavaScript**, arguments are replaced before the script executes:

- `%1` through `%9` - Individual space-separated arguments (no `%0` support)

**Example:**
```
Alias: attack
Command:
Execute As: JavaScript
```

```javascript
var target = "%1";
var spell = "%2";
terminal.Send("cast '" + spell + "' " + target);
```

**Usage:**
- Type: `attack orc fireball`
- Result: `%1` is replaced with "orc", `%2` is replaced with "fireball"
- Sends: `cast 'fireball' orc` to the game

**Important:** In JavaScript aliases, argument replacement happens before script execution. Any unused `%n` placeholders are replaced with empty strings.

---

## Examples

### Example 1: Simple Command Alias

Create a shortcut for getting gold from a container and depositing it at the bank:

```
Alias: banking
Command: open pack;get all.gold pack;deposit all;close pack
Execute As: Command
Enabled: True
```

**Usage:** Type `banking` in the input box.

---

### Example 2: JavaScript Alias with Conditional Logic

Create a spell that will cast heal until the player is fully healed.

```
Alias: full-heal
Execute As: JavaScript
Enabled: True
```

```javascript
let hp = int.Parse(terminal.GetVariable("Health"))
let maxHp = int.Parse(terminal.GetVariable("MaxHealth"))
let mana = int.Parse(terminal.GetVariable("Mana"))

let diffHp = maxHp - hp

terminal.EchoLine(`Difference to heal is {W${diffHp}{xhp.`)

// Loop until we're fully healed.
while (hp < maxHp) {
    // Sanity check
    if (mana < 50) {
        terminal.EchoLine("You don't have enough mana to continue")
        return
    }
    
    // Do the heal, then pause for long enough that these don't stack up.
    terminal.Send("c heal")
    await ui.PauseAsync(4000)
    
    // Get the new values of the stats.
    hp = int.Parse(terminal.GetVariable("Health"))
    maxHp = int.Parse(terminal.GetVariable("MaxHealth"))
    mana = int.Parse(terminal.GetVariable("Mana"))    
    diffHp = maxHp - hp
    
    terminal.EchoLine(`Difference to heal is {W${diffHp}{xhp.`)
}
```

**Usage:** Type `full-heal` in the input box.

---

### Example 3: Character-Specific Alias

Create an alias that only works for your mage character:

```
Alias: mb
Command: cast 'magic missile' @target
Execute As: Command
Character: Merlin
Enabled: True
```

In this example `@target` is a variable that has been set either by alias or in the Variables dialog.  You could use `%1` if you wanted to pass a manual in.

**Usage:** Type `mb` when playing as Merlin. The alias will not trigger for other characters.

---

### Example 4: Group Combat Alias

Create a combat alias assigned to a group for easy management:

```
Alias: kts
Command: kill @target;backstab @target
Execute As: Command
Group: Combat
Enabled: True
```

**Usage:** All combat aliases can be enabled or disabled together by managing the "Combat" group.

---

### Example 5: Button-Enabled Alias

Create an alias with a visible button for quick access:

```
Alias: gq
Command: group;qui;affects
Execute As: Command
Button Label: Status
Button Visible: True
Sort Order: 1
```

**Usage:** Click the "Status" button or type `gq` in the input box.

---

### Example 6: Alias with Arguments (Command)

Create a flexible casting alias that accepts spell name and target as arguments:

```
Alias: c
Command: cast '%1' %2
Execute As: Command
Enabled: True
```

**Usage:** 
- Type `c fireball orc` to cast fireball at an orc
- Type `c heal` to cast heal (no second argument needed)
- The `%1` becomes the spell name, `%2` becomes the target

---

### Example 7: Alias with Arguments (JavaScript)

Create a JavaScript alias that validates and uses arguments:

```
Alias: smartcast
Execute As: JavaScript
Enabled: True
```

```javascript
var spell = "%1";
var target = "%2";

if (!spell) {
    terminal.EchoLine("Usage: smartcast <spell> [target]");
    return;
}

var mana = parseInt(terminal.GetVariable("mana"));
if (mana < 20) {
    terminal.EchoLine("Not enough mana!");
    return;
}

if (target) {
    terminal.Send("cast '" + spell + "' " + target);
} else {
    terminal.Send("cast '" + spell + "'");
}
```

**Usage:** Type `smartcast heal` or `smartcast fireball orc` to cast spells with mana checking.

---

### Example 8: JavaScript Alias with Client API

Create an alias that uses the scripting API to send multiple commands with delays:

```
Alias: autocraft
Execute As: JavaScript
Group: Crafting
Enabled: True
```

```javascript
terminal.EchoLine("Starting auto-craft sequence...");
terminal.Send("get hammer pack");
await ui.PauseAsync(1000)
terminal.Send("craft sword");
await ui.PauseAsync(2000)
terminal.Send("put sword pack");
terminal.EchoLine("Crafting complete!");
```

**Usage:** Type `autocraft` to execute the crafting sequence with built-in delays.

---

## See Also

- [Scripting](Scripting.md) - Learn more about JavaScript scripting capabilities
- [Variables](Variables.md) - Understanding how to use variables in aliases
- [Triggers](Triggers.md) - Automated responses to game output
- [Macros](Macros.md) - Keyboard shortcuts for commands
- [Packages](Packages.md) - Organizing and sharing aliases

---

[Return to Table of Contents](Index.md)
