# Avalon Mud Client

## Lua Extension Commands

An extension command is one that was written in C# but exposed to Lua.  These generally will provide some ease of use functionality or allow Lua to interact with the UI in some way.  Lua extensions are prefixed with a namespace like:

```
local dir = lua:RandomChoice({"north", "south"});
```

|Command|Parameters|Description|
|-------|----------|-----------|
|lua:Send|(string) command|Sends a command to the game.|
|lua:GetVariable|(string) key|Gets a global variable|
|lua:SetVariable|(string) key, (string) Value|Sets a global variable.|
|lua:Echo|(string) text|Echos text to the terminal window.|
|lua:Echo|(string) msg, (string color), (bool) reverse|Echos text to the terminal window in a given color.|
|lua:EchoEvent|(string) text|Echos text to the terminal window.|
|lua:EchoWindow|(string) windowName, (string) text|Appends text to a pop up terminal window.  The window must already exist with the specified name.|
|lua:ClearWindow|(string) windowName|Clears the text in a popup terminal window of the same name.|
|lua:Coalesce|Value one (string), Value two (string)|Returns the first non null and non empty value.|
|lua:GetTime|None|Gets the current time in HH:MM:SS format|
|lua:GetHour|None|Gets the current hour.|
|lua:GetMinute|None|Gets the current minute.|
|lua:GetSecond|None|Gets the current second.|
|lua:GetMillisecond|None|Gets the current millisecond.|
|lua:Sleep|milliseconds (int)|Pauses a Lua script for the specified set of milliseconds (e.g. 1000 = 1 second)|
|lua:RandomNumber|Low value (int), High value (int)|Returns a random number|
|lua:RandomChoice|Values (string array)|Returns a random value from the provide list of values in the string array|
|lua:Guid|None|Returns a unique global unique identifier value (GUID)|
|lua:SetTitle|title (string)|Sets the title of the mud client window.|
|lua:GetScrapedText|None|Gets the value of the current screen scrape buffer.|
|lua:Contains|text (string), search text (string)|Determines if one string contains another.  Case sensitive.|
|lua:Contains|text (string), search text (string), ignore case (bool)|Determines if one string contains another with the option to ignore case.|
|lua:Trim|text (string)|Trims whitespace off the front and end of a string.|
|lua:Trim|text (string), trim value (string)|Trims a specified value off the front and end of a string.|
|lua:TrimStart|text (string)|Trims whitespace off the front of a string.|
|lua:TrimStart|text (string), trim value (string)|Trims a specified value off the start of a string.|
|lua:TrimEnd|text (string)|Trims whitespace off the end of a string.|
|lua:TrimEnd|text (string), trim value (string)|Trims a specified value off the end of a string.|
|lua:Split|text (string), delimiter (string)|Splits text into an array using a provided delimiter|
|lua:ArrayContains|array (string array), search value (string)|Searches an array for the occurance of a specified string|
|lua:Replace|text (string), search text (string, replacement text (string)|Replaces some text with a replace in a string.
|lua:EnableGroup|group name (string)|Enables a group of aliases and triggers.|
|lua:DisableGroup|group name (string)|Disables a group of aliases and triggers.|
|lua:AddScheduledTask|command (string), is lua (bool), seconds (int)|Schedules a command to run in 'n' seconds.|
|lua:AddBatchTask|command (string), is lua (bool)|Adds a command to a batch that will execute when `lua:StartBatch` is called.|
|lua:StartBatch|Seconds in between commands (int)|Starts a batch and runs the command with 'N' seconds in between them.|
|lua:ClearTasks|None|Clears all scheduled tasks.|
|lua:FormatNumber|value (string)|Formats a number with commas.|
|lua:FormatNumber|value (string), decimal places (int)|Formats a number with the specified number of decimal places.|
|lua:ListAdd|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Adds an item to a list.|
|lua:ListAddStart|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Adds a item to the beginning of a list.|
|lua:ListAddIfNotExist|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Adds an item to a list only if it does not exist.|
|lua:ListRemove|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Removes all items from a list that match.|
|lua:ListRemove|(string) sourceList, (int) items, (optional char) delimiter = '\|')|Removes 1 to n items from the end of a list|
|lua:ListExists|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Whether or not an item exists in a list.|
|lua:ListSort|(string) sourceList, (bool) ascending, (optional char) delimiter = '\|'|Sorts a list in ascending or descending order.|
|lua:LastNonEmptyLine|None|Gets the last non empty line.|
|lua:LastLines|(int) numberToTake|Returns the requested number of lines from the end of the game terminall as a string array from oldest to newest.|
|lua:LastLines|(int) numberToTake, (bool)reverseOrder|Returns the requested number of lines from the end of the game terminal as a string array.  `reverseOrder` being true will return the list in newest to oldest order, false will return oldest to newest.|
|global|None|Used to set global variables only global to Lua.|

## Lua Global Variables

Lua global variables can be used by prefixing a `global` namespace.  Example:

```
global.my_name = "Rhien"
global.counter = 0
```

Supported types in global variables

- String
- Number
- Boolean
- Table
- Void/Nil