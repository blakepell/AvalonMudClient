# Avalon Mud Client

## Aliases

An alias is a command that you create that can execute multiple other commands to the game or run a Lua script.  An alias has the following properties:

|Property|Description|
|--------|-----------|
|Name|The name of the alias that you would enter to execute it.|
|Command|The command, commands or Lua script that should be executed.|
|Enabled|Whether or not the current alias is enabled and can be used.|
|Character|If the `Character` variable is set, the character that must be active in order for the alias to be usable even if Enabled.  This allows you to have aliases that are only available for given characters if you play multiple characters on a game.|
|Group|A group name that can be used to group sets of aliases and triggers together.  The `#group-enable` and `#group-diable` hash commands can be used to enable or disable sets of triggers and aliases by the group name you define.|
|ExecuteAs|Whether the command should be run as a `Command` or run through a script engine like Lua.|
|Lock|Lock an alias prevents it from being auto-updated if you import a Package that has an alias of the same name.  If a game you play provides pre-set triggers and aliases you might find that you want to customize some of them but not have them over-written if you update a package that has many aliases/triggers in it.|
|Count|This is the number of times the alias has been executed.|

Aliases can be globally disabled on the `Alias` window/tab or via the Quick option menu.

![alt text](/doc/images/AliasGlobalEnable1.png "Global Alias Enable #1")
![alt text](/doc/images/AliasGlobalEnable2.png "Global Alias Enable #2")

## Alias Arguments

<!-- An alias can pass it's arguments in a number of manors.  Simple aliases that execute as commands can use the tradition %n notation.  E.g. `some-alias %1 %2`  -->

A simple alias to cast a spell and then eat created food might look like:

```
Name: food
Command: c 'create food';get mushroom;eat mushroom
```

A simple alias that executes some Lua might look as follows:

```
Name: olc-set-field

-- Edits all vnums in a range and sets their sector to 'field'
for i = %1, %2, 1 
do
    lua:Send("edit room " .. i)
    lua:Send("sector field")
end
```

In the above Lua example you might notice %1 and %2.  These represent the parameters you use after the alias and are passed in order.  `olc-set-field 1230 1240` would loop over all numbers (vnums in this case) between 1230 and 1240 and execute a command in each of those rooms.

A more likely example of a shorthand alias with parameters might look like this which will cast an acid blast spell on a target.  It's usage would be `a Rhien` to cast an acid blast spell on a character named `Rhien`.

```
Name: a
Command: c 'acid blast' %1
```

### Variables

Aliases can also reference variables that you set and can view/edit through the `Variables` window/tab. A variable can be manually added, set via the `#set` hash command or via `Lua.SetVariable` Lua command.  A variable is referenced in an alias by putting an `@` in front of it's name.

Below is an example that would echo out a variable by the name of `Person`.

```
Name: say-hi
#echo Hello @Person
```

It should be noted at this time that the variable is swapped in at the time the initial commands are executed.  This behavior may change at a future time so that they would execute after each individual command is run.  At this time, if an alias should set a variable and then execute a command based off of that variable a simple Lua scripts can be used like in the next example.

```
-- Sets the Variable Person equal to the first parameter
lua.SetVariable("Person", "%1")

-- For examples sake, take that variable and put it into a local Lua variable.
local personName = lua.GetVariable("Person")

-- Echo it out so we can see that it was set.
lua.Echo("You have set the 'Person' variable equal to " .. personName)
```

### Search Tips:
The alias data grid editor has a filter option at the of the panel that will default to searching the alias expression, command, character or group.  Additonal search options are available as defined the switches included below that allow you to specify which of the extended fields to search.

```
function:<function name>
```
```
id:<ID of alias>
```
```
package:<ID of package that the alias is associated with>
```
