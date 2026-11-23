# UI Elements
---

UI Elements are prebuilt interactive components that help you navigate and customize your view.

Each of these elements is a tool window. They can be opened from the **Layout** button in the **UI** group on the **Game** tab of the ribbon, and like every other window they can be docked, floated, tabbed or auto hidden wherever you like. See [Layouts](Layouts.md) for how to arrange them.

Most UI elements show information for the **active profile**, the game terminal you're currently using. When you switch to another game's tab, they switch with it.

## Alias Button Actions

The **Alias Button Actions** window holds a button for each [alias](Aliases.md) you choose. Clicking a button runs its alias exactly as if you had typed the alias's name into the command box. It's a good place for commands you use often but don't want to put on a [macro](Macros.md) key, or for things you'd like to be able to do with the mouse.

To give an alias a button, open the alias and set these settings in its **UI** category:

| Setting | Description |
|---------|-------------|
| **Button Visible** | Shows a button for the alias. |
| **Button Label** | The text shown on the button. Keep it short, since long labels are cut off. |

Because a button runs the alias by name, it works with any alias, including aliases that run JavaScript. If an alias has a **Character** set, its button is only shown while you're playing that character.

| Example Alias | Button Label | What the Button Does |
|---------------|--------------|----------------------|
| `recall` → `recite recall` | `Recall` | Gets you out of trouble with a single click. |
| `buffs` → `c armor;c bless;c sanctuary` | `Buffs` | Casts all of your protective spells. |
| `combat-on` → `#group-enable combat` | `Combat On` | Turns on a group of [triggers](Triggers.md). |

The width of the buttons is set with **Alias Button Width** in the profile settings (the default is `100`), and the space around them is set with **Button Margin** in the client settings.

## Affects

The **Affects** window lists the spells and other effects that are currently on your character, along with how long each one has left. Each affect has a bar that drains as it wears off and changes color to warn you:

| Color | Time Left |
|-------|-----------|
| Green | 4 or more ticks. |
| Yellow | 2 to 3 ticks. |
| Red | 1 tick or less. |

Affects with no time limit show **Permanent**, and affects whose length isn't known show **?**. The list is sorted so the longest lasting affects are at the top. Each game tick, every timed affect counts down by one and is removed when it runs out.

Double click an affect to cast it again. The client sends `c '<affect name>'` to the game.

### Where Affects Come From

The client doesn't read affects from the game text on its own. Something has to tell it what you're affected by:

- On games that send affect information over [GMCP](GMCP.md), the **GMCP** package installed when you first run Avalon fills in the Affects window automatically.
- The **Affects** [package](Packages.md) adds helper scripts such as `is_affected("haste")`, plus the `clear-affects` alias to empty the list.
- On other games, a [trigger](Triggers.md) that runs JavaScript can add affects itself:

```javascript
let af = new Affect();
af.Name = "sanctuary";
af.Duration = 12;
vm.Affects.AddOrUpdateAffect(af);
```

Adding an affect that's already in the list updates its time left rather than adding it twice. `vm.Affects.RemoveAffect("sanctuary")` removes one and `vm.Affects.Clear()` removes them all.

## Variables

The **Variables** window shows the [variables](Variables.md) you choose and keeps them up to date as their values change. It's a good way to watch things like your current room, your target or your gold without having to type a command to check.

Open a variable and set these settings to control how it appears:

| Setting | Description |
|---------|-------------|
| **Visible** | Shows the variable in the Variables window. |
| **Label** | A friendly name to show instead of the variable's key, for example `Target` instead of `CurrentTarget`. |
| **Foreground Color** | The color of the label and value text, such as `#FFFFFF`. |
| **Background Color** | The color behind the label and value, such as `#0089bd`. |
| **Horizontal Alignment** | Whether the value is aligned to the left, center or right. |

Scripts can show, hide and color variables as they change:

```javascript
terminal.SetVariable("Target", "goblin");
terminal.ShowVariable("Target");

// Set a variable and its text color at the same time.
terminal.SetVariable("Health", "15%", "#FF0000");

terminal.HideVariable("Target");
```

## Movement

The **Movement** window is a set of buttons laid out like a compass. Click a button to move in that direction in the active game.

| Button | Sends |
|--------|-------|
| **NW**, **N**, **NE**, **W**, **E**, **SW**, **S**, **SE** | `nw`, `n`, `ne`, `w`, `e`, `sw`, `s`, `se` |
| **U** and **D** | `u` and `d` |
| The map button in the center | Opens the quick [directions](Directions.md) window, which lists the places you can walk to from where you are. |

The movement buttons are useful when you're playing with the mouse or on a touch screen. If you'd rather move with the keyboard, see the number pad [macros](Macros.md#number-pad-movement).

## Log Window

The **Log Window** shows messages from the client itself, as opposed to text from the game. Check here when something isn't working the way you expect. You'll find things like script errors, a trigger that failed, packages being installed and new terminals being created.

| Column | Description |
|--------|-------------|
| **Type** | How important the message is: `Debug`, `Info`, `Success`, `Warning` or `Error`. |
| **Time** | When the message was logged. |
| **Count** | When the same message is logged several times in a row, it's shown once and this counts how many times it happened. |
| **Message** | The message itself. |

Right click a message to **Copy** it, **Clear All** messages, or **View Exception Details** for an error. If a message came from a trigger or alias, double click it to open that trigger or alias so you can fix it.

Two client settings in the **Logging** category control the Log Window:

| Setting | Description |
|---------|-------------|
| **Log Level** | The least important type of message to keep. The default is `Info`, which hides `Debug` messages. |
| **Log Capacity** | The most messages the window holds before the oldest ones are removed. The default is `1000`. |

Scripts can write their own messages to the Log Window, which is useful when you're debugging a script and don't want to clutter the game terminal:

```javascript
terminal.Log("Loot script ran");
```

## New Terminal

A **New Terminal** is an extra terminal window that you can send text to. Use one to pull text you care about out of the main game window, so it doesn't scroll away in the middle of a fight. Common uses are tells, group chat, immortal channels, loot or the output of your own scripts. The **Communication** window is a terminal of this kind that's already set up for you, and the Channels package sends channel messages to it.

### Creating a Terminal

Click **Terminal** in the **UI** group on the **Game** tab of the ribbon and choose **New Terminal** (it's also on the **Layout** button as **Add New Terminal**). Enter a name and the terminal appears as a new tool window. Its name is what you'll use to send text to it, so keep it short and leave out spaces, for example `Wiznet` or `Loot`.

You don't have to create a terminal by hand before using it. When a script writes to a terminal that doesn't exist yet, the client creates it for you. Either way, once it exists you can [dock](Layouts.md#the-docking-system) it wherever you like, and it's put back in the same place the next time the client starts.

Each terminal has buttons to clear and search it. Right click it to **Copy** text, **Copy with Mud Colors**, **Clear Terminal** or **Open in Regex Builder**, which is handy when you're writing a trigger for a line you've captured.

### Sending Text to a Terminal

Text is sent to a terminal with `terminal.WriteLine` from the [scripting](Scripting.md) environment. Give it the name of the terminal and the text to write:

```javascript
terminal.WriteLine("Loot", "You picked up a shiny sword.");
```

Each line starts with a timestamp. To leave it off, pass `false` as a third argument:

```javascript
terminal.WriteLine("Loot", "You picked up a shiny sword.", false);
```

The text can include the color codes from [Themes](Themes.md#the-terminal-color-settings), such as `{G` for green and `{x` to reset the color.

`WriteLine` is usually called from a [trigger](Triggers.md) with **Execute As** set to `JavaScript`, which moves a line from the game into the terminal. `%1`, `%2` and so on are replaced with the text the trigger's pattern captured. For example, this trigger copies Wiznet messages into a terminal named `Wiznet` and adds the game's port when the `GamePort` variable is set, which helps when you're connected to more than one copy of a game:

```javascript
let gamePort = terminal.GetVariable("GamePort");

if (string.IsNullOrWhiteSpace(gamePort)) {
    terminal.WriteLine("Wiznet", `[ {GWiznet{x ] %1`)
}
else {
    terminal.WriteLine("Wiznet", `[ {GWiznet{x ] ${gamePort} %1`)
}
```

With a pattern like `^Wiznet: (.*)$`, the line `Wiznet: Bob has entered the game.` appears in the Wiznet terminal as `[ Wiznet ] Bob has entered the game.`, with "Wiznet" in green.

Turn on the trigger's **Gag** setting as well if you want the line moved out of the main game window rather than copied.

Other terminal functions you may find useful:

| Function | Description |
|----------|-------------|
| `terminal.Clear("Wiznet")` | Clears the named terminal. |
| `terminal.AddTerminal("Wiznet", "containerName", autoHide, side)` | Creates a terminal and docks it next to the named window, or along a side of the client if no window has that name. |
| `#echo -t Wiznet Hello` | Writes a line to an existing terminal from the command box, an alias or a trigger, without a script. |

[Return to Table of Contents](Index.md)
