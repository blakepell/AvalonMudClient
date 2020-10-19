# Avalon Mud Client

## Lua Hash Commands

A hash command is a built in mud client command that can be run as a command without Lua.  They typically allow you to enter a command to interact with the environment.  This might be to send an operating system toast message, flash the window background, writing to a text file, echo to the terminal or change/interact with the UI in some way.

Many hash commands support switches like they were built for use in an operating system command prompt.  E.g.

```
#group-enable Leveling
#group-disable Leveling
#echo -c Cyan -r Your chaining yourself bro.  Abort Abort.
#append-to-file -f "C:\Temp\Toasts.txt" -n -t "%1"
```

Hash commands that have switches/parameters can all have the `--help` switch used to get example usage and what they support.

```
#echo --help

#echo: Echos the text to the terminal in the specified color.

  -c, --color      The name of the supported color the echo should rendered as.
  -r, --reverse    Whether or not to reverse the colors.  Only works when a
                   valid color is specified.
  --help           Display this help screen.
  value pos. 0     The text to echo to the terminal window.
```

Hash commands that reside in the main project will be general purpose.  Plugins though may implement very specific hash commands that are tailored for a given mud.  As an example, The example dsl-mud.org plugin DLL contains one called "#scan-all" which will scan in all the directions available in a given room (if only north and south are avaiable, it will only scan north and south).

## Hash Command List

|Command|Description|
|-------|-----------|
|#alias|Executes an alias with the option to delay sending it to the game by a specified amount.|
|#append-to-file|Appends text to a specified file.|
|#beep|Makes a beep sound with one of a set of OS specified beeps.|
|#clear|Clears the terminal window or any of the specified panels that contain text.|
|#compass|Shows a compass window and allows for setting the direction of the needle.|
|#cmd-list|Lists all of the available hash commands.|
|#dir|Shells the quick directions window.|
|#disable|Global disables of all triggers or all aliases.|
|#echo|Echos text to the terminal.  Supports ANSI colors in Lopes style format.  e.g. {R = Red|
|#echo-event|Echos and event the default or any specified terminal.|
|#enable|Global enable or aliases or triggers.|
|#end|Exits the application|
|#flash|Flashes the mud window on the taskbar|
|#gc|Memory garbage collection (use with caution)|
|#get|Gets a global variable value|
|#group-disable|Disables an entire group of aliases and triggers|
|#group-enable|Enables an entire group of aliases and triggers|
|#is-connected|Whether the client has an active TCP/IP connection open.|
|#line-count|Shows the number of lines in the terminal window|
|#lua|Executes Lua asynchronously.|
|#lua-debug|Provides debugging information about the current Lua environment.|
|#lua-sync|Executes Lua synchronously.|
|#macro|Will execute a macro.|
|#pulse|Pulses the input text box a color like a heart beat.|
|#recent-triggers|Will show you the recent triggers that have fired and the date/time they fired on.|
|#repeat|Will repeat a command 'n' number of times. @counter and @index can be used to send the iteration you're on to the command executed.|
|#replace|Replaces all occurances of specified text in a terminal with another piece of text.  This can be a long operation if the terminal window has a large portion of data.  Consider #replace-first and #replace-last where possible.|
|#replace-first|Replaces the first occurance of specified text in a terminal with another piece of text.|
|#replace-last|Replaces the last occurance of specified text in a terminal with another piece of text.|
|#save|Saves your profile.|
|#scrape|Turns screen scraping on or off.|
|#scroll-to-end|Scrolls the terminal buffer to the last line.|
|#set|Sets a global variable.|
|#set-title|Sets the title of the window to something other than 'Avalon Mud Client'.|
|#shell|Shells and OS program or command.|
|#sql-execute|Excutes a SQL command against the profiles SQLite database.|
|#task-add|Adds a command to to be executed in a specified amount of time.  This is a single usage timer/scheduler.|
|#task-clear|Clears all pending tasks.|
|#task-list|Echos out all pending tasks.|
|#toast|Shows an operating system toast message.|
|#toast-alarm|Shows an operating system toast message in alarm format meaning it will stay around longer.|
|#trigger|Allows for a single trigger to be manipulated (enabled, disabled, etc.)|
|#update-info-bar|Updates the info bar with the current values of it's underlaying model.|
|#version|Echos the version of the Avalon Mud Client|
|#walk|Fast walks a given path|
|#var|Allows for the manipulation of variables.|
|#window|Allows the caller to set the main windows height/width, left/right, title, center the window on the screen or echo window information.  The caller may also create new terminal windows with this command that can be written to.|

## DSL Plugin Hash Commands

|Command|Description|
|-------|-----------|
|#edit|Edits an existing mob prog in the custom mob prog editor.  The editor has intellisense for code completion and documentation for what each command does.<br />Syntax:#edit mp &lt;vnum&gt;|
|#dsl-version|Echos the version of the DSL plugin.|
|#online|Retrieves who is playing the game as per the dsl-mud.org web-site.|
|#con-card|Checks on the status of a submitted con card.|
|#scan-all|Scans in the directions that are available from the current room.|
|#wiki|Starts a browser and runs a search on the DSL Wiki.|
|#remove-affect|Removes an affect by name from the current affects list.|
|#partial-affect|Adds or updates an affect whose duration or modifiers aren't known.|
|#open-all|Attempts to open doors in all directions.|