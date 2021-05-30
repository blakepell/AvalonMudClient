# Avalon Mud Client

## Variables

There are two main types of variables in Avalon.  The first are global variables that are saved with the profile and will persist across sessions (These are CLR variables).  These variables can be viewed and editted on the `Variables` tab, via the `#set` and `#get` hash commands and with the Lua `lua:GetVariable` and `lua:SetVariable` commands.  These variables can be use to persist state and are available from CLR plugins (plugins that are written in C# and compile to DLL's).

The second type of variable are global variables in Lua.  These variables are accessible to all Lua scripts but will cease to exist when the mud client is closed.  These are stored as Lua types and accessed through the `global` namespace.  

### CLR Global Variable Example:

```
#set Target Rhien
#get Target
```

CLR variables can also be set via triggers which is an incredibly useful mechanism.  A regular expression trigger with a named value will automatically put that named match into a variable.

### CLR Variable set from a Regular Expression

```
\<(?<Health>\d+)/(?<MaxHealth>\d+)hp (?<Mana>\d+)/(?<MaxMana>\d+)m (?<Move>\d+)/(?<MaxMove>\d+)mv \((?<Wimpy>\d+)\|(?<Stance>\w+)\) \((?<Room>.*?)\) \((?<ExitsShort>.*?)\) (?<ExpTnl>.*?) (?<GameTime>.*?)\>
```

The above trigger would match the following prompt.  It would set each of the named variables and store it for later use.  In this instance it sets hit points, mana, movement, the name of the room your in, the obvious exits, experiance and the in game time.  This is the most important example of this technique because your other triggers, aliases and Lua scripts now have access to these variables and you can have those scripts make decisions based off of this data.  Perhaps when your hit points get too low you would cast a heal spell or you want to log the room name into a database if it doesn't exist.

```
<1825/1902hp 542/652m 276/376mv (100|Offensive) (The Cross Roads) (NESW) 47906 11:30am>
```

### Built-in CLR Variables

|Variable|Description|
|--------|-----------|
|@Username|The username of the person currently logged into the computer|
|@Date|The currently date in a time friendly format.|

### Variable Repeater

On the main window of the mud client there is a variable repeater.  It can be made visible or not visible on the `...` menu in the upper right hand area of the mud client.  When a CLR variable is created you can edit it via the `Variables` window which can be accessed via the `Edit->Edit Variables` menu or by pressing the `Control+Alt+V` hot key.  By check the `Visible` check box on the variable you want it will then always show with it's updated value on the main mud window making it a handy way to show important game data.  Those variables can be sorted via the `Display Order` value.  The order will be `Display Order` lowest to highest then alphabetical.  If all `Display Order` values are 0 then the sort defaults alphabetical.

### Variable Events

The following events are available for use with variables.  Events in Avalon will execute Lua scripts if they are provided.

|Event|Description|Notes|
|-----|-----------|-----|
|OnChange|Executes when the variable's value changes.  If the value is set to the same value the variable already is this event will not fire.|In the OnChange Lua script, %1 will have the old value substituted in and %2 will have the new value substituted in.|

**Note** In the variable editor, the Lua scripts must have the `Save` button clicked in order for the Lua script to be updated.  The Lua editor does not support data binding that automatically updated values as the other field on this form.  This is desirable as it helps to prevent partially coded Lua scripts from executing. 

### Important Variables of Note

The following variables have special importance in the mud client and when sent enable special functionality.

|Variable|Description|
|--------|-----------|
|Room|The room name your character is in.  This allows for the game to pull walking paths from the `Directions` you've input that are walkable from the room you're in.|
|Character|The name of the character you're currently playing.  Having the character name set allows you to have aliases and triggers that only run for that specific character, effecitvly allowing you to use a single profile for multiple characters.|
|Health|The *current* HP or hit points your character has.|
|MaxHealth|The *maximum* HP or hit points your chracter has.|
|Mana|The *current* MANA or magic your character has.|
|MaxMana|The *maximum* MANA or magic your character has.|
|Move|The *current* MOVE or movement your character has.|
|MaxMove|The *maximum* MOVE or movement your character has.|
|Stance|The battle stance your chracter is in, may not apply to many muds but can be used to surface other varibles to the InfoBar.|
|ExitsShort|The exits to the room in short format (NSEW)|

Once these are set the `#update-info-bar` hash command can be called to update the InfoBar.  This bar does not currently real-time bind although that might be something that is considered in the future.

### Lua Global Variable Example:

```
global.my_name = "Rhien"
global.counter = global.counter + 1
```

Supported types in Lua global variables are:

- String
- Number
- Boolean
- Table
- Void/Nil

Although this is a simple concept and implementation they'll provide you an incredibly useful way to allow aliases, triggers, timers and tasks to work together.

### Lua OnChange Event

When a variable changes the mud client will check to see if it has an OnChange event defined.  If it does, it will execute the Lua code for that event.  In the code
the old value is swapped in for `%1` and the new value is swapped in for `%2`.  The following example shows how when a weight value is read in and set via a trigger
how a variable repeater variable can then be set to show the weight and max weight as one field to save space.

```
-- We know this is the weight variable, get the max weight so we can then format
-- a new variable and THAT will be surfaced on the UI saving space by only having
-- one item for weight instead of two for weight and max weight.
local max_weight = lua.GetVariable("MaxWeight")
local buf = "%2/" .. max_weight
lua.SetVariable("DisplayWeight", buf)
```

### Notes:

- If a CLR variable does not exist when it's requested a blank string will be returned.
- A Lua variable on the other hand can and will return nil.