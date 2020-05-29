# Avalon Mud Client

## Lua Tips

### Echos

When writing more than one line to the mud client with commands like `lua.Echo` it is much more efficient to create your
entire string in Lua and then make one `lua.Echo` call.  The reason behind this is that each call gets marshalled to the
UI thread which is an expensive operation.  Note that when echoing multiline strings your colors will need to be started
 on each line.  Lines in the string maybe seperated with the C style carriage return/line feed `\r\n`.

### Mud Client Variables

Although Avalon Lua supports globals, Lua can also reference variables which the mud client sets that are stored on the
C# side.  Variables though simple are one of the more powerful constructs you'll use when writing scripts.  Imagine a
trigger that sets a variable with a creatures name you see in a game.  A long running lua script may then use that variable
as it becomes available effectivly letting the script react to in game content.

### Pausing your script

One of the common things Lua coders need is a way to pause their script in a way that doesn't block the mud client.  To
make this easy for you `lua.Sleep` has been provided as a way for you to pause your script without blocking the execution
of anything else in the mud client.  `lua.Sleep` accepts milliseconds for the parameter.  This is handy for cases when
you're running a looped action that must continue for a long time but might be waiting for input from the game.  In order
to interact with the loop you can use Lua global variable that are set from other commands.