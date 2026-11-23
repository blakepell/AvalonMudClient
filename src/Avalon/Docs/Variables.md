# Variables
| Property | Description |
| --- | --- |
| Key | The unique name of the variable. This identifier is used to reference the variable in scripts and bindings. |
| Value | The current value of the variable. |
| Character | (Optional) The character associated with this variable. Enables variable scoping per character. |
| Display Order | The order in which the variable appears in lists and UIs. Lower numbers appear first. |
| Label | A friendly display name for the variable. If set, this label is shown instead of the key. |
| Foreground Color | The color used for the variable’s text in the variable display.  The color format is in hexadecimal format (e.g. `#20964A` ). |
| Background Color | The color used for the variable’s background in the variable display. The color format is in hexadecimal format (e.g. `#EA467E`)|
| OnChange Event | JavaScript code to execute when the variable’s value changes. |
| Package | The package name to which this variable belongs. Used for organizing and exporting profiles. |
| Visible | Whether the variable is shown in the UI. When unchecked, the variable is hidden from variable displays. |

## Variable Usage

Variables are typically used from either a simple alias, a command trigger or from a script (aliases and triggers can both execute scripts instead of simple commands).  Variables are profile specific.

For an example, let's assume we have a variable named `Target`.

### Simple Command Usage

``` js
examine @Target
```

### Usage from a Script

There are two ways to get a variable from a script that I will demonstrate.  The first way uses a helper untility on the terminal that will return a single string value for a matching variable.  The second way uses a LINQ query to return the entire `Variable` object which will give you access to everything the Variable has stored with it (this allows for complex scripting scnearios).

#### terminal.GetVariable

``` js
// Read the Target variable into a value
let value = terminal.GetVariable("Target")
terminal.Send(`examinme ${Name}`)
```

#### LINQ query of all variables

``` js
// Return the first value with a Key of "Gold".  If none are found
// then FirstOrDefault will return a null.
let v = profile.Variables.FirstOrDefault(x => x.Key == "Gold")


// Check our results to see what we found.
if (v != null) {
    ui.Alert(`You have ${v.Value} gold.`)
}
else {
    ui.Alert("We could not determine how much gold you have.")
    return;
}

// Turn the string value into a number
let gold = int.Parse(v.Value)

// If the gold is greater than 0, make it visible in the variable repeater 
// UI element, otherwise hide it.
v.IsVisible = (gold > 0);
```

### Usage by Looping over all profile variables

In this example, we should the key/pair values for all variables in the current profile.

``` js
let i = 0;

terminal.EchoLine("Current Variable List")
terminal.EchoLine("---------------------")

for (const v of profile.Variables) {
    i++
    terminal.EchoLine(`${i}.) ${v.Key} = ${v.Value}`)
}
```

[Return to Table of Contents](Index.md)
