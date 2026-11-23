# Hash Commands
---

Hash commands are commands for the client itself.  They start with a `#` and are handled by Avalon instead of being sent to the game.  They can be typed into the command box or used as the command of an alias, trigger, macro or scheduled task.  Type `#cmd-list` in a terminal to see every command.

|Command|Description|
|-------|-----------|
| `#echo [-d] <text>` | Echoes text (ANSI color codes allowed) to the terminal.  `-d` prefixes a timestamp. |
| `#send <text>` | Sends text to the game exactly as typed, skipping aliases. |
| `#clear` | Clears the terminal. |
| `#set <variable> <value>` | Sets a variable. |
| `#get <variable>` | Echoes a variable. |
| `#var <name> [-s value] [-a n] [-m n] [-i] [-d] [-e] [--delete]` | Sets, adds to, subtracts from, increments, decrements, echoes or deletes a variable. |
| `#list <vars\|aliases\|triggers\|directions\|macros\|scripts>` | Lists the items of one kind. |
| `#enable <triggers\|aliases\|gmcp>` / `#disable ...` | Turns all triggers, aliases or GMCP on or off. |
| `#group-enable <group>` / `#group-disable <group>` | Enables or disables the aliases and triggers in a group. |
| `#edit <name>` | Opens the editor for an alias, trigger or script by its name or ID. |
| `#script <function> [args]` | Calls a script function.  `#script -r` refreshes the scripting environment. |
| `#walk <speedwalk>` | Walks a speedwalk such as `2n e (open door) 3w`. |
| `#go <direction>` | Walks a saved direction, from the `Room` variable when it is set. |
| `#dir` | Shows the directions available from the current room. |
| `#reverse-path <speedwalk>` | Echoes a speedwalk reversed. |
| `#repeat <count> <command>` | Repeats a command. |
| `#task-add -c "command" -s <seconds> [-j]` | Runs a command (or JavaScript with `-j`) after a delay. |
| `#task-list` / `#task-clear` | Lists or clears scheduled tasks. |
| `#toast <message>` / `#toast-alarm <seconds> <message>` | Shows a notification now or after a delay. |
| `#flash` | Flashes the window on the task bar. |
| `#shell <program> [args]` / `#shell-url <url>` | Starts a program or opens a web page. |
| `#append-to-file -f <file> -t "text" [-n]` | Appends text to a file (relative paths are in Documents\Avalon). |
| `#save` | Saves the settings. |
| `#line-count`, `#recent-triggers`, `#globals`, `#debug`, `#sql-debug`, `#is-connected`, `#version`, `#guid`, `#gc` | Diagnostics. |
| `#null` | Does nothing. |
| `#end` | Saves and closes the client. |

[Return to the Table of Contents](Index.md)
