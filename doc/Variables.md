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

### Notes:

- If a CLR variable does not exist when it's requested a blank string will be returned.
- A Lua variable on the other hand can and will return nil.