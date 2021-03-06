# Avalon Mud Client

## Lua Extension Commands

An extension command is one that was written in C# but exposed to Lua.  These generally will provide some ease of use functionality or allow Lua to interact with the UI in some way.  Lua extensions are prefixed with a namespace like:

```
local dir = lua.RandomChoice({"north", "south"});
```

|Command|Parameters|Description|
|-------|----------|-----------|
|lua.Send|(string) command|Sends a command to the game.|
|lua.GetVariable|(string) key|Gets a global variable|
|lua.SetVariable|(string) key, (string) value|Sets a global variable.|
|lua.SetVariable|(string) key, (string) value, (string) color|Sets a global variable.|
|lua.ShowVariable|(string) key|Shows a variable in the variable repeater if the key is found.|
|lua.HideVariable|(string) key|Hides a variable in the variable repeater if the key is found.
|lua.RemoveVariable|(string) key|Removes a global variable.|
|lua.Echo|(string) text|Echos text to the terminal window.|
|lua.Echo|(string) msg, (int) terminal|Echos text to the specified terminal in the main client window.|
|lua.Echo|(string) msg, (string color), (bool) reverse|Echos text to the terminal window in a given color.|
|lua.EchoEvent|(string) text|Echos text to the terminal window.|
|lua.EchoWindow|(string) windowName, (string) text|Appends text to a pop up terminal window.  The window must already exist with the specified name.|
|lua.ClearWindow|(string) windowName|Clears the text in a popup terminal window of the same name.|
|lua.Coalesce|Value one (string), Value two (string)|Returns the first non null and non empty value.|
|lua.GetTime|None|Gets the current time in HH:MM:SS format|
|lua.GetTime|(bool) meridiemTime|Whether to get the time in a 12 or 24 hour format.|
|lua.GetHour|None|Gets the current hour.|
|lua.GetMinute|None|Gets the current minute.|
|lua.GetSecond|None|Gets the current second.|
|lua.GetMillisecond|None|Gets the current millisecond.|
|lua.DailyMillisecondsElapsed|None|Gets the number of milliseconds that have elapsed since midnight.|
|lua.DailyMinutesElapsed|None|Gets the number of minutes that have elapsed since midnight.|
|lua.DailyHoursElapsed|None|Gets the number of hours that have elapsed since midnight.|
|lua.Sleep|milliseconds (int)|Pauses a Lua script for the specified set of milliseconds (e.g. 1000 = 1 second)|
|lua.RandomNumber|Low value (int), High value (int)|Returns a random number|
|lua.RandomChoice|Values (string array)|Returns a random value from the provide list of values in the string array|
|lua.Guid|None|Returns a unique global unique identifier value (GUID)|
|lua.SetTitle|title (string)|Sets the title of the mud client window.|
|lua.SetText|text (string)|Sets the text on the specified UI element.  If the target is omitted the main status bar label is used.|
|lua.SetText|text (string), iconName (string)|Sets the text on the specified UI element.  If the target is omitted the main status bar label is used.|
|lua.SetText|text (string), target (int),  iconName (string)|Sets the text on the specified UI element.  If the target is omitted the main status bar label is used.|
|lua.GetScrapedText|None|Gets the value of the current screen scrape buffer.|
|lua.Contains|text (string), search text (string)|Determines if one string contains another.  Case sensitive.|
|lua.Contains|text (string), search text (string), ignore case (bool)|Determines if one string contains another with the option to ignore case.|
|lua.Trim|text (string)|Trims whitespace off the front and end of a string.|
|lua.Trim|text (string), trim value (string)|Trims a specified value off the front and end of a string.|
|lua.TrimStart|text (string)|Trims whitespace off the front of a string.|
|lua.TrimStart|text (string), trim value (string)|Trims a specified value off the start of a string.|
|lua.TrimEnd|text (string)|Trims whitespace off the end of a string.|
|lua.TrimEnd|text (string), trim value (string)|Trims a specified value off the end of a string.|
|lua.Split|text (string), delimiter (string)|Splits text into an array using a provided delimiter|
|lua.ArrayContains|array (string array), search value (string)|Searches an array for the occurance of a specified string|
|lua.Replace|text (string), search text (string, replacement text (string)|Replaces some text with a replace in a string.
|lua.EnableGroup|group name (string)|Enables a group of aliases and triggers.|
|lua.DisableGroup|group name (string)|Disables a group of aliases and triggers.|
|lua.AddScheduledTask|command (string), is lua (bool), seconds (int)|Schedules a command to run in 'n' seconds.|
|lua.AddBatchTask|command (string), is lua (bool)|Adds a command to a batch that will execute when `lua.StartBatch` is called.|
|lua.StartBatch|Seconds in between commands (int)|Starts a batch and runs the command with 'N' seconds in between them.|
|lua.ClearTasks|None|Clears all scheduled tasks.|
|lua.FormatNumber|value (string)|Formats a number with commas.|
|lua.FormatNumber|value (string), decimal places (int)|Formats a number with the specified number of decimal places.|
|lua.ListAdd|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Adds an item to a list.|
|lua.ListAddStart|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Adds a item to the beginning of a list.|
|lua.ListAddIfNotExist|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Adds an item to a list only if it does not exist.|
|lua.ListRemove|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Removes all items from a list that match.|
|lua.ListRemove|(string) sourceList, (int) items, (optional char) delimiter = '\|')|Removes 1 to n items from the end of a list|
|lua.ListExists|(string) sourceList, (string) value, (optional char) delimiter = '\|'|Whether or not an item exists in a list.|
|lua.ListSort|(string) sourceList, (bool) ascending, (optional char) delimiter = '\|'|Sorts a list in ascending or descending order.|
|lua.LastNonEmptyLine|None|Gets the last non empty line.|
|lua.LastLines|(int) numberToTake|Returns the requested number of lines from the end of the game terminall as a string array from oldest to newest.|
|lua.LastLines|(int) numberToTake, (bool)reverseOrder|Returns the requested number of lines from the end of the game terminal as a string array.  `reverseOrder` being true will return the list in newest to oldest order, false will return oldest to newest.|
|lua.LogInfo|(string) msg|Logs an informational log message.|
|lua.LogWarning|(string) msg|Logs a warning log message.|
|lua.LogError|(string) msg|Logs an error log message.|
|lua.LogSuccess|(string) msg|Logs a success log message.|
|lua.MD5|(string) value|Returns an MD5 hash for the specified string value.|
|lua.SHA256|(string) value|Returns an SHA256 hash for the specified string value.|
|lua.SHA512|(string) value|Returns an SHA512 hash for the specified string value.|
|lua.ProfileDirectory|None|The location of where the profile save directory.|
|lua.AppDataDirectory|None|The location of where the core AppData directory.|
|lua.RemoveLinesStartingWith|(string) text, (string) searchValue|Removes all lines from the string that start with the specified search value.|
|lua.RemoveLinesEndingWith|(string) text, (string) searchValue|Removes all lines from the string that end with the specified search value.|
|lua.RemoveNonAlpha|(string) text, (string) includeAlso|Removes non alpha characters but allows for an exceptions list of chars to be provided that should be included.|
|lua.RemoveNonAlpha|(string) text|Removes non alpha characters from a string.|
|lua.StartsWith|(string) text, (string) searchText|If a string starts with another string.|
|lua.EndsWith|(string) text, (string) searchText|If a string ends with another string.|
|lua.LuaScriptsActive||The number of Lua scripts that are actively running.|
|lua.DbExecute|(string) sql, (object) params|Executes a parameterized SQL statement intended for write operations.|
|lua.DbExecuteImmediate|(string) sql, (object) params|Executes a SQL command immediately outside of a transaction.  Used for scenarios where statements can't run in a transaction such as `CREATE TABLE`.|
|lua.DbSelect|(string) sql, (object) params|Executes a parameterized SQL statement and returns a record set as a Lua table that can be iterated over.|
|lua.DbSelectValue|(string) sql, (object) params|Executes a parameterized SQL statement and returns a single value (the first column of the first row).|
|lua.HttpGet|(string) urlDownloads a string using an HTTP GET.|
|lua.HttpPost|(string) url, (string) data|Downloads a string using an HTTP POST.  The data variable is a key pair formatted like: 'name=Rhien hobby=muds'|
|lua.CaptureOn||Turns on text capturing which will capture the incoming game text with ANSI color codes removed.|
|lua.CaptureOff||Turns off text capturing.|
|lua.CaptureClear||Clears the text capture buffer.|
|lua.Left|(string) str, (int) length|Returns the specified number of characters from the left side of the string. If more characters were requested than exist the full string is returned.|
|lua.Right|(string) str, (int) length|Returns the specified number of characters from the right side of the string. If more characters were requested than exist the full string is returned.|
|lua.Substring|(string) str, (int) startIndex|
|lua.Substring|(string) str, (int) startIndex, (int) length|Returns the substring starting at the specified index.|
|lua.IndexOf|(string) str, (string) search|Returns the zero based index of the first occurrence of a string in another string.|
|lua.IndexOf|(string) search, (int(string) str,) startIndex|Returns the zero based index of the first occurrence of a string in another string.|
|lua.IndexOf|(string) str, (string) search, (int) startIndex, (int) length|Returns the zero based index of the first occurrence of a string in another string.|
|lua.LastIndexOf|(string) str, (string) search|Returns the zero based index of the last occurrence of a string in another string.|
|lua.LastIndexOf|(string) search, (int(string) str,) startIndex|Returns the zero based index of the last occurrence of a string in another string.|
|lua.LastIndexOf|(string) str, (string) search, (int) startIndex, (int) length|Returns the zero based index of the last occurrence of a string in another string.|
|lua.RemoveElementsEmpty|string[] array|Removes all empty/whitespace elements from an array.|
|lua.RemoveElementsContains|string[] array, string str|Removes all elements from an array that contain the specified text.|
|lua.RemoveElementsEndingWith|string[] array, string str|Removes all elements from an array that end with the specified text.|
|lua.RemoveElementsStartsWith|string[] array, string str|Removes all elements from an array that start with the specified text.|
|lua.LastLinesBetweenContains|string startLineContains|Removes all elements from an array between the last occurrence of lines that contain a start and optional ending pattern.|
|lua.LastLinesBetweenContains|string startLineContains, string endLineContains|Removes all elements from an array between the last occurrence of lines that contain a start and optional ending pattern.|
|lua.LastLinesBetweenStartsWith|string startLineStartsWith|Removes all elements from an array between the last occurrence of lines that start with a start and optional ending pattern.|
|lua.LastLinesBetweenStartsWith|string startLineStartsWith, string endLineStartsWith|Removes all elements from an array between the last occurrence of lines that start with a start and optional ending pattern.|
|lua.RemoveAnsiCodes|string[] array|Removes all ANSI codes from a string or array.|
|lua.RemoveAnsiCodes|string str|Removes all ANSI codes from a string or array.|
|lua.IsNumber|string value|Returns whether the string is a number.|
|lua.IsEven|int value|Returns whether a provided number is even.|
|lua.IsOdd|int value|Returns whether a provided number is odd.|
|lua.IsInterval|int value, int interval|Returns whether the number is of a specified interval.|
|lua.Clamp|int value, int min, int max|Returns the value if it falls in the range of the max and min.  Otherwise it returns the upper or lower boundary depending on which one the value passed|
|lua.DeleteLeft|string value, int length|Deletes the specified number of characters off the start of the string.  If the length is greater than the length of the string an empty string is returned.|
|lua.DeleteRight|string value, int length|Deletes the specified number of characters off the end of the string.  If the length is greater than the length of the string an empty string is returned.|
|lua.FirstWord|string value|Returns the first word in the specified string.|
|lua.SecondWord|string value|Returns the second word in the specified string.|
|lua.ThirdWord|string value|Returns the third word in the specified string.|
|lua.ParseWord|string value, int wordNumber, int delimiter|Returns the word by index from the provided string as delimited by spaces.  The delimiter can also be provided to specify a different split character.|
|lua.RemoveWord|string value, int wordNumber|Returns a string with the specified word removed by index.|
|lua.Between|string value, string beginMarker, string endMarker|Returns the string between the start marker and the end marker.|
|lua.ToBase64|string value|Converts a string to Base64.|
|lua.FromBase64|string value|Converts a Base64 string back to it's original state.|
|lua.HtmlEncode|string value|HTML encodes a string.|
|lua.HtmlDecode|string value|HTML decodes a string.|
|lua.UrlEncode|string value|URL encodes a string.|
|lua.UrlDecode|string value|URL decodes a string.|
|lua.WordCount|string value|Returns the word count in the specified string.|
|lua.PadLeft|string value|Returns a string that right aligns the instance by padding characters onto the the left until the total width is attained.  If the total width is less than the provided string the provided string is returned.|
|lua.PadRight|string value|Returns a string that right aligns the instance by padding characters onto the the left until the total width is attained.  If the total width is less than the provided string the provided string is returned.|
|global|None|Used to set global variables only global to Lua.|Returns the substring starting at the specified index for the specified length.|

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

## DSL Plugin Lua Extensions

DSL lua commands are prefixed with the `dsl.` qualifier.

|Command|Parameters|Description|
|-------|----------|-----------|
|dsl.IsAffected|(string) affectName|A boolean true or false on whether the player is affected by an affect.|
|dsl.AffectDuration|(string) affectName|The duration left in game ticks of the specified affect.|
